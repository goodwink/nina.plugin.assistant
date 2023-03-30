﻿using Assistant.NINAPlugin.Database;
using Assistant.NINAPlugin.Database.Schema;
using Assistant.NINAPlugin.Plan;
using NINA.Core.Utility;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Collections.Concurrent;
using System.Data.Entity.Migrations;
using System.Text;
using System.Threading;

namespace Assistant.NINAPlugin.Sequencer {

    /// <summary>
    /// Watch for saved images and update the associated exposure plan in the scheduler database.
    /// 
    /// This class works in conjunction with AssistantTakeExposure (the scheduler version of the core
    /// NINA instruction to take an exposure).  AssistantTakeExposure.Execute will call WaitForExposure
    /// to register an image Id with the associated exposure plan database Id and tell the watcher that
    /// it can't stop until all image Ids have come through the pipeline.
    /// 
    /// This is crucial: we have to update the exposure plan database for each exposure while handling
    /// the asychronous nature of the NINA image save pipeline.  We also have to ensure we've processed
    /// them all before allowing this watcher to be stopped (which means the planner will be called again
    /// immediately).
    /// 
    /// </summary>
    public class ImageSaveWatcher : IImageSaveWatcher {

        private IImageSaveMediator imageSaveMediator;
        private ConcurrentDictionary<int, int> exposureDictionary;

        private IPlanTarget planTarget;
        private bool enableGrader;

        public ImageSaveWatcher(IImageSaveMediator imageSaveMediator, IPlanTarget planTarget) {
            this.imageSaveMediator = imageSaveMediator;
            exposureDictionary = new ConcurrentDictionary<int, int>(Environment.ProcessorCount * 2, 31);
            this.planTarget = planTarget;
            this.enableGrader = planTarget.Project.EnableGrader;
        }

        public void Start() {
            exposureDictionary.Clear();
            imageSaveMediator.ImageSaved += ImageSaved;
            Logger.Debug($"Scheduler: start watching image saves for {planTarget.Project.Name}/{planTarget.Name}");
        }

        public void WaitForExposure(int imageId, int exposurePlanDatabaseId) {
            Logger.Debug($"Scheduler: registering waitFor exposure: iId={imageId} eId={exposurePlanDatabaseId}");
            exposureDictionary.TryAdd(imageId, exposurePlanDatabaseId);
        }

        public void Stop() {
            Logger.Debug($"Scheduler: stopping image save watcher, waiting for exposures to complete:\n{ExposureIdsLog()}");

            // We need to wait on all exposures to process (and the database to be updated) before we can stop.
            // Otherwise, the planner will be called again with the exposure plan counts not reflecting the
            // latest exposures.  Poll every 1/5 sec and bail out after 10 secs.

            int count = 0;
            while (!exposureDictionary.IsEmpty) {
                if (++count == 50) {
                    Logger.Warning("Timed out waiting on all exposures to be processed and scheduler database updated");
                    break;
                }

                Thread.Sleep(200);
            }

            imageSaveMediator.ImageSaved -= ImageSaved;
            Logger.Debug($"Scheduler: stopped watching image saves for {planTarget.Project.Name}/{planTarget.Name}");
        }

        private void ImageSaved(object sender, ImageSavedEventArgs msg) {
            if (msg.MetaData.Image.ImageType != "LIGHT") {
                return;
            }

            int? imageId = msg.MetaData?.Image?.Id;
            bool accepted = enableGrader ? new ImageGrader().GradeImage(planTarget, msg) : false;
            Logger.Debug($"Scheduler: image save for {planTarget.Project.Name}/{planTarget.Name}, filter={msg.Filter}, grader enabled={enableGrader}, accepted={accepted}, image id={imageId}");

            UpdateDatabase(planTarget, msg.Filter, accepted, msg, imageId);

            if (imageId != null) {
                int old;
                exposureDictionary.TryRemove((int)imageId, out old);
            }

            Logger.Debug($"Scheduler: ImageSaved: id={imageId}");
        }

        private void UpdateDatabase(IPlanTarget planTarget, string filterName, bool accepted, ImageSavedEventArgs msg, int? imageId) {

            using (var context = new SchedulerDatabaseInteraction().GetContext()) {
                using (var transaction = context.Database.BeginTransaction()) {

                    try {
                        if (imageId != null) {

                            // Update the exposure plan record
                            int exposureDatabaseId;
                            exposureDictionary.TryGetValue((int)imageId, out exposureDatabaseId);
                            ExposurePlan exposurePlan = context.GetExposurePlan(exposureDatabaseId);
                            if (exposurePlan != null) {
                                exposurePlan.Acquired++;

                                if (accepted) { exposurePlan.Accepted++; }
                                context.ExposurePlanSet.AddOrUpdate(exposurePlan);
                            }
                            else {
                                Logger.Warning($"Scheduler: failed to get exposure plan for id={exposureDatabaseId}, image id={imageId}");
                            }
                        }
                        else {
                            Logger.Warning("Scheduler: no image id to determine exposure plan database id?!");
                        }

                        // Save the acquired image record
                        AcquiredImage acquiredImage = new AcquiredImage(
                            planTarget.Project.DatabaseId,
                            planTarget.DatabaseId,
                            msg.MetaData.Image.ExposureStart,
                            filterName,
                            accepted,
                            new ImageMetadata(msg));
                        context.AcquiredImageSet.Add(acquiredImage);

                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception e) {
                        Logger.Error($"Scheduler: exception updating database for saved image: {e.Message}\n{e.StackTrace}");
                    }
                }
            }
        }

        private string ExposureIdsLog() {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in exposureDictionary) {
                sb.AppendLine($"{entry.Key}: {entry.Value}");
            }

            return sb.ToString();
        }
    }

    public class ImageSaveWatcherEmulator : IImageSaveWatcher {

        public void Start() {
            Logger.Debug("Scheduler ImageSaveWatcherEmulator Start");
        }

        public void WaitForExposure(int imageId, int exposurePlanDatabaseId) {
            Logger.Debug($"WaitForExposure: iId={imageId} eId={exposurePlanDatabaseId}");
        }

        public void Stop() {
            Logger.Debug("Scheduler ImageSaveWatcherEmulator Stop");
        }
    }

}

