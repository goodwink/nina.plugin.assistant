﻿using Assistant.NINAPlugin.Database;
using Assistant.NINAPlugin.Database.Schema;
using Assistant.NINAPlugin.Plan;
using Assistant.NINAPlugin.Plan.Scoring.Rules;
using Moq;
using NINA.Plugin.Assistant.Test.Astrometry;
using NINA.Plugin.Assistant.Test.Plan;
using NINA.Profile.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Text;

namespace NINA.Plugin.Assistant.Test.Database {

    [TestFixture]
    public class GenerateTestDatabase {

        private AssistantDatabaseInteraction db;

        [SetUp]
        public void SetUp() {
            db = GetDatabase();
        }

        [Test]
        [Ignore("tbd")]
        public void TomTest1() {
            string profileId = "3c160865-776f-4f72-8a05-5964225ca0fa"; // Zim
            using (var context = db.GetContext()) {
                try {
                    Project p1 = new Project(profileId);
                    p1.Name = "Project: M42";
                    p1.Description = "";
                    p1.State = ProjectState.Active;
                    p1.ActiveDate = new DateTime(2022, 12, 1);
                    p1.StartDate = p1.ActiveDate;
                    p1.EndDate = new DateTime(2023, 2, 1);
                    // TODO: set new project prefs here
                    /*
                    AssistantProjectPreferencesOLD p1Prefs = new AssistantProjectPreferencesOLD();
                    p1Prefs.SetDefaults();
                    p1Prefs.MinimumAltitude = 10;
                    SetDefaultRuleWeights(p1Prefs);
                    p1.preferences = new ProjectPreferenceOLD(p1Prefs);
                    */

                    Target t1 = new Target();
                    t1.Name = "M42";
                    t1.ra = TestUtil.M42.RADegrees;
                    t1.dec = TestUtil.M42.Dec;
                    p1.Targets.Add(t1);

                    FilterPlan fp = new FilterPlan(profileId, "Ha");
                    fp.Desired = 3;
                    fp.Exposure = 20;
                    fp.Gain = 100;
                    fp.Offset = 10;
                    t1.FilterPlans.Add(fp);
                    fp = new FilterPlan(profileId, "OIII");
                    fp.Desired = 3;
                    fp.Exposure = 20;
                    fp.Gain = 100;
                    fp.Offset = 10;
                    t1.FilterPlans.Add(fp);
                    fp = new FilterPlan(profileId, "SII");
                    fp.Desired = 3;
                    fp.Exposure = 20;
                    fp.Gain = 100;
                    fp.Offset = 10;
                    t1.FilterPlans.Add(fp);

                    context.ProjectSet.Add(p1);

                    Project p2 = new Project(profileId);
                    p2.Name = "Project: IC1805";
                    p2.Description = "";
                    p2.State = ProjectState.Active;
                    p2.ActiveDate = new DateTime(2022, 12, 1);
                    p2.StartDate = p2.ActiveDate;
                    p2.EndDate = new DateTime(2023, 2, 1);

                    // TODO: set new project prefs here
                    /*
                    AssistantProjectPreferencesOLD p2Prefs = new AssistantProjectPreferencesOLD();
                    p2Prefs.SetDefaults();
                    p2Prefs.MinimumAltitude = 10;
                    SetDefaultRuleWeights(p2Prefs);
                    p2.preferences = new ProjectPreferenceOLD(p2Prefs);
                    */

                    Target t2 = new Target();
                    t2.Name = "IC1805";
                    t2.ra = TestUtil.IC1805.RADegrees;
                    t2.dec = TestUtil.IC1805.Dec;
                    p2.Targets.Add(t2);

                    fp = new FilterPlan(profileId, "Ha");
                    fp.Desired = 5;
                    fp.Exposure = 20;
                    fp.Gain = 100;
                    fp.Offset = 10;
                    t2.FilterPlans.Add(fp);
                    fp = new FilterPlan(profileId, "OIII");
                    fp.Desired = 5;
                    fp.Exposure = 20;
                    fp.Gain = 100;
                    fp.Offset = 10;
                    t2.FilterPlans.Add(fp);
                    fp = new FilterPlan(profileId, "SII");
                    fp.Desired = 5;
                    fp.Exposure = 20;
                    fp.Gain = 100;
                    fp.Offset = 10;
                    t2.FilterPlans.Add(fp);

                    context.ProjectSet.Add(p2);

                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "Ha"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "OIII"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "SII"));

                    //ImageMetadata imd = new ImageMetadata(PlanMocks.GetImageSavedEventArgs(DateTime.Now, "Ha"));
                    //AcquiredImage ai = new AcquiredImage(1, DateTime.Now, "Ha", imd);
                    //context.AcquiredImageSet.Add(ai);

                    context.SaveChanges();

                    ReadAndDump(profileId, new DateTime(2023, 1, 1));
                }
                catch (DbEntityValidationException e) {
                    StringBuilder sb = new StringBuilder();
                    foreach (var eve in e.EntityValidationErrors) {
                        foreach (var dbeve in eve.ValidationErrors) {
                            sb.Append(dbeve.ErrorMessage).Append("\n");
                        }
                    }

                    TestContext.WriteLine($"DB VALIDATION EXCEPTION: {sb.ToString()}");
                    throw e;
                }
                catch (Exception e) {
                    TestContext.WriteLine($"OTHER EXCEPTION: {e.Message}\n{e.ToString()}");
                    throw e;
                }
            }
        }

        [Test]
        //[Ignore("")]
        public void RealTest1() {

            using (var context = db.GetContext()) {
                try {
                    DateTime atTime = new DateTime(2023, 1, 26);
                    string profileId = "3c160865-776f-4f72-8a05-5964225ca0fa"; // Zim
                    //string profileId = "1f78fa60-ab20-41af-9c17-a12016553007"; // Astroimaging Redcat 51 / ASI1600mm

                    Project p1 = new Project(profileId);
                    p1.Name = "Project: C00";
                    p1.Description = "";
                    p1.State = ProjectState.Active;
                    p1.ActiveDate = atTime.AddDays(-1);
                    p1.StartDate = atTime;
                    p1.EndDate = atTime.AddDays(100);
                    p1.MinimumTime = 60;
                    p1.MinimumAltitude = 22;
                    p1.UseCustomHorizon = false;
                    p1.HorizonOffset = 0;
                    p1.FilterSwitchFrequency = 1;
                    p1.DitherEvery = 2;
                    SetDefaultRuleWeights(p1);

                    Target t1 = new Target();
                    t1.Name = "C00";
                    t1.ra = TestUtil.C00.RA;
                    t1.dec = TestUtil.C00.Dec;
                    p1.Targets.Add(t1);

                    t1.FilterPlans.Add(GetFilterPlan(profileId, "Lum", 5, 0, 60));
                    t1.FilterPlans.Add(GetFilterPlan(profileId, "Red", 5, 0, 60));
                    t1.FilterPlans.Add(GetFilterPlan(profileId, "Green", 5, 0, 60));
                    t1.FilterPlans.Add(GetFilterPlan(profileId, "Blue", 5, 0, 60));

                    Project p2 = new Project(profileId);
                    p2.Name = "Project: C90";
                    p2.Description = "";
                    p2.State = ProjectState.Active;
                    p2.ActiveDate = atTime.AddDays(-1);
                    p2.StartDate = atTime;
                    p2.EndDate = atTime.AddDays(100);

                    SetDefaultRuleWeights(p2);

                    Target t2 = new Target();
                    t2.Name = "C90";
                    t2.ra = TestUtil.C90.RA;
                    t2.dec = TestUtil.C90.Dec;
                    p2.Targets.Add(t2);

                    t2.FilterPlans.Add(GetFilterPlan(profileId, "Ha", 5, 0, 90));
                    t2.FilterPlans.Add(GetFilterPlan(profileId, "OIII", 5, 0, 90));
                    t2.FilterPlans.Add(GetFilterPlan(profileId, "SII", 5, 0, 90));

                    context.ProjectSet.Add(p1);
                    context.ProjectSet.Add(p2);

                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "Lum"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "Red"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "Green"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "Blue"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "Ha"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "OIII"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "SII"));

                    context.SaveChanges();
                }
                catch (Exception e) {
                    TestContext.Error.WriteLine($"failed to create test database: {e.Message}\n{e.ToString()}");
                    throw e;
                }
            }
        }

        [Test]
        [Ignore("")]
        public void DaytimeTest1() {

            using (var context = db.GetContext()) {
                try {
                    DateTime atTime = new DateTime(2023, 1, 28);
                    string profileId = "3c160865-776f-4f72-8a05-5964225ca0fa"; // Zim
                                                                               //string profileId = "1f78fa60-ab20-41af-9c17-a12016553007"; // Astroimaging Redcat 51 / ASI1600mm

                    Project p1 = new Project(profileId);
                    p1.Name = "Project: C00";
                    p1.Description = "";
                    p1.State = ProjectState.Active;
                    p1.ActiveDate = atTime.AddDays(-1);
                    p1.StartDate = atTime;
                    p1.EndDate = atTime.AddDays(100);
                    p1.MinimumTime = 30;
                    p1.MinimumAltitude = 0;
                    p1.UseCustomHorizon = false;
                    p1.HorizonOffset = 0;
                    p1.FilterSwitchFrequency = 0;
                    p1.DitherEvery = 0;
                    p1.EnableGrader = true;

                    Target t1 = new Target();
                    t1.Name = "C00";
                    t1.ra = TestUtil.C00.RA;
                    t1.dec = TestUtil.C00.Dec;
                    p1.Targets.Add(t1);

                    t1.FilterPlans.Add(GetFilterPlan(profileId, "L", 5, 0, 60));
                    t1.FilterPlans.Add(GetFilterPlan(profileId, "R", 5, 0, 60));
                    //t1.filterplans.Add(GetFilterPlan(profileId, "G", 5, 0, 60));
                    //t1.filterplans.Add(GetFilterPlan(profileId, "B", 5, 0, 60));

                    context.ProjectSet.Add(p1);

                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "L"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "R"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "G"));
                    context.FilterPreferencePlanSet.Add(new FilterPreference(profileId, "B"));

                    context.SaveChanges();
                }
                catch (Exception e) {
                    TestContext.Error.WriteLine($"failed to create test database: {e.Message}\n{e.ToString()}");
                    throw e;
                }
            }
        }

        private void SetDefaultRuleWeights(Project project) {
            Dictionary<string, IScoringRule> rules = ScoringRule.GetAllScoringRules();
            foreach (KeyValuePair<string, IScoringRule> entry in rules) {
                var rule = entry.Value;
                project.ruleWeights.Add(new RuleWeight(rule.Name, rule.DefaultWeight));
            }
        }

        private AssistantDatabaseInteraction GetDatabase() {
            var testDbPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"assistantdb.sqlite");
            TestContext.WriteLine($"DB PATH: {testDbPath}");
            return new AssistantDatabaseInteraction(string.Format(@"Data Source={0};", testDbPath));
        }

        private FilterPlan GetFilterPlan(string profileId, string filterName, int desired, int accepted, int exposure) {
            FilterPlan fp = new FilterPlan(profileId, filterName);
            fp.Desired = desired;
            fp.Accepted = accepted;
            fp.Exposure = exposure;
            fp.Gain = 100;
            fp.Offset = 10;
            return fp;
        }

        private List<IPlanProject> ReadAndDump(string profileId, DateTime atTime) {

            List<Project> projects = null;
            List<FilterPreference> filterPrefs = null;

            AssistantDatabaseInteraction database = GetDatabase();
            using (var context = database.GetContext()) {
                try {
                    projects = context.GetActiveProjects(profileId, atTime);
                    filterPrefs = context.GetFilterPreferences(profileId);
                }
                catch (Exception ex) {
                    TestContext.WriteLine($"Assistant: exception accessing Assistant: {ex}");
                }
            }

            if (projects == null || projects.Count == 0) {
                return null;
            }

            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestUtil.TEST_LOCATION_4);
            profileMock.SetupProperty(m => m.ActiveProfile.Id, new Guid(profileId));
            List<IPlanProject> planProjects = new List<IPlanProject>();

            Dictionary<string, FilterPreference> dict = new Dictionary<string, FilterPreference>();
            foreach (FilterPreference filterPref in filterPrefs) {
                dict.Add(filterPref.FilterName, filterPref);
            }
            Dictionary<string, FilterPreference> filterPrefsDictionary = dict;

            foreach (Project project in projects) {
                PlanProject planProject = new PlanProject(profileMock.Object.ActiveProfile, project, filterPrefsDictionary);
                planProjects.Add(planProject);
                TestContext.WriteLine($"PROJECT:\n{planProject}");
            }

            return planProjects;
        }
    }
}
