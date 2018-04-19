﻿using ASC.Web.Core;
using ASC.Web.Projects.Core;
using Autofac;
using NUnit.Framework;

namespace ASC.Web.Projects.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASC.Core;
    using ASC.Projects.Core.Domain;
    using ASC.Projects.Engine;

    [TestFixture]
    public class BaseTest
    {
        protected ILifetimeScope Scope { get; set; }
        protected TaskEngine TaskEngine { get; set; }
        protected MilestoneEngine MilestoneEngine { get; set; }
        protected MessageEngine MessageEngine { get; set; }
        protected TimeTrackingEngine TimeTrackingEngine { get; set; }
        protected ProjectEngine ProjectEngine { get; set; }
        protected ParticipantEngine ParticipantEngine { get; set; }
        protected DataGenerator DataGenerator { get; set; }

        private Project Project { get; set; }

        public static Guid UserInTeam        = new Guid("0d5ed025-a78c-48b6-8ec9-29b225e85e23");
        public static Guid ProjectManager    = new Guid("4bf9ca85-4565-45a1-ac18-7827aad06685");
        public static Guid UserNotInTeam     = new Guid("e4308b59-90bd-4f6c-807e-d1ee7716fe2d");
        public static Guid Guest             = new Guid("a9367768-30da-49a3-97f6-61c96b53c914");
        public static Guid Admin             = new Guid("93580c54-1132-4d6b-bf2d-da0bfaaa1a28");
        public static Guid Owner             = new Guid("646a6cff-df57-4b83-8ffe-91a24910328c");

        [OneTimeSetUp]
        public void Init()
        {
            WebItemManager.Instance.LoadItems();

            CoreContext.TenantManager.SetCurrentTenant(0);
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            SecurityContext.AuthenticateMe(tenant.OwnerId);

            Scope = DIHelper.Resolve(true);

            var engineFactory = Scope.Resolve<EngineFactory>();
            ProjectEngine = engineFactory.ProjectEngine;
            ParticipantEngine = engineFactory.ParticipantEngine;
            TaskEngine = engineFactory.TaskEngine;
            MilestoneEngine = engineFactory.MilestoneEngine;
            MessageEngine = engineFactory.MessageEngine;
            TimeTrackingEngine = engineFactory.TimeTrackingEngine;
            DataGenerator = new DataGenerator();

            var team = new List<Guid>(2) { ProjectManager, UserInTeam, Guest };

            Project = SaveOrUpdate(GenerateProject(ProjectManager));
            AddTeamToProject(Project, team);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Delete(Project);

            if (Scope != null)
            {
                Scope.Dispose();
            }
        }

        protected Project GenerateProject(Guid userId)
        {
            return DataGenerator.GenerateProject(userId);
        }

        protected Project SaveOrUpdate(Project project)
        {
            return ProjectEngine.SaveOrUpdate(project, false);
        }

        protected Project Get(Project project)
        {
            return ProjectEngine.GetByID(project.ID);
        }

        protected void AddTeamToProject(Project project, List<Guid> team)
        {
            foreach (var user in team.Select(r => ParticipantEngine.GetByID(r)))
            {
                ProjectEngine.AddToTeam(project, user, false);
            }
        }

        protected List<Guid> GetTeam(int projectId)
        {
            return ProjectEngine.GetTeam(projectId).Select(r => r.ID).ToList();
        }

        protected void Delete(Project project)
        {
            ProjectEngine.Delete(project.ID);
        }


        protected Task GenerateTask()
        {
            return DataGenerator.GenerateTask(Project);
        }

        protected Task SaveOrUpdate(Task task)
        {
            return TaskEngine.SaveOrUpdate(task, new List<int>(), false);
        }

        protected Task Get(Task task)
        {
            return TaskEngine.GetByID(task.ID);
        }

        protected void Delete(Task task)
        {
            TaskEngine.Delete(task);
        }

        protected Milestone GenerateMilestone()
        {
            return DataGenerator.GenerateMilestone(Project);
        }

        protected Milestone SaveOrUpdate(Milestone milestone)
        {
            return MilestoneEngine.SaveOrUpdate(milestone);
        }

        protected Milestone Get(Milestone milestone)
        {
            return MilestoneEngine.GetByID(milestone.ID);
        }

        protected void Delete(Milestone milestone)
        {
            MilestoneEngine.Delete(milestone);
        }


        protected Message GenerateMessage()
        {
            return DataGenerator.GenerateMessage(Project);
        }

        protected Message SaveOrUpdate(Message message)
        {
            return MessageEngine.SaveOrUpdate(message, false, new List<Guid>());
        }

        protected Message Get(Message message)
        {
            return MessageEngine.GetByID(message.ID);
        }

        protected void Delete(Message message)
        {
            MessageEngine.Delete(message);
        }

        protected TimeSpend GenerateTimeTracking()
        {
            var task = SaveOrUpdate(GenerateTask());
            return DataGenerator.GenerateTimeTracking(task);
        }

        protected TimeSpend SaveOrUpdate(TimeSpend timeSpend)
        {
            return TimeTrackingEngine.SaveOrUpdate(timeSpend);
        }

        protected TimeSpend Get(TimeSpend timeSpend)
        {
            return TimeTrackingEngine.GetByID(timeSpend.ID);
        }

        protected void Delete(TimeSpend timeSpend)
        {
            TimeTrackingEngine.Delete(timeSpend);
        }

        protected void ChangeProjectPrivate(bool @private)
        {
            SecurityContext.AuthenticateMe(Owner);
            Project.Private = @private;
            SaveOrUpdate(Project);
        }

        protected void ChangeProjectStatus(ProjectStatus status)
        {
            SecurityContext.AuthenticateMe(Owner);
            Project.Status = status;
            SaveOrUpdate(Project);
        }

        protected void RestrictAccess(Guid userID, ProjectTeamSecurity projectTeamSecurity, bool visible)
        {
            SecurityContext.AuthenticateMe(Owner);
            ProjectEngine.SetTeamSecurity(Project, ParticipantEngine.GetByID(userID), projectTeamSecurity, visible);
        }
    }
}