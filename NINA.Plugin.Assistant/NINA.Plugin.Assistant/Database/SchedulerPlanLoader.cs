﻿using Assistant.NINAPlugin.Database.Schema;
using Assistant.NINAPlugin.Plan;
using Assistant.NINAPlugin.Util;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;

namespace Assistant.NINAPlugin.Database {

    public class SchedulerPlanLoader {

        public List<IPlanProject> LoadActiveProjects(SchedulerDatabaseContext context, IProfile activeProfile) {
            List<Project> projects = null;
            string profileId = activeProfile.Id.ToString();

            using (context) {
                try {
                    projects = context.GetActiveProjects(profileId);
                }
                catch (Exception ex) {
                    throw ex; // let the caller decide how to handle
                }
            }

            if (projects == null || projects.Count == 0) {
                TSLogger.Warning("Assistant: no projects are active at planning time");
                return null;
            }

            bool haveActiveTargets = false;
            foreach (Project project in projects) {
                foreach (Target target in project.Targets) {
                    if (target.Enabled) {
                        foreach (ExposurePlan plan in target.ExposurePlans) {
                            if (plan.Desired > plan.Accepted) {
                                haveActiveTargets = true;
                                break;
                            }
                        }
                    }
                    if (haveActiveTargets) { break; }
                }

                if (haveActiveTargets) { break; }
            }

            if (!haveActiveTargets) {
                TSLogger.Warning("Assistant: no targets with exposure plans are active for active projects at planning time");
                return null;
            }

            List<IPlanProject> planProjects = new List<IPlanProject>();
            foreach (Project project in projects) {
                PlanProject planProject = new PlanProject(activeProfile, project);
                planProjects.Add(planProject);
            }

            return planProjects;
        }

    }
}
