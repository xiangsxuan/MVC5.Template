﻿using MvcTemplate.Components.Logging;
using MvcTemplate.Data.Core;
using MvcTemplate.Objects;
using MvcTemplate.Tests.Data;
using MvcTemplate.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace MvcTemplate.Tests.Unit.Components.Logging
{
    [TestFixture]
    public class LoggableEntryTests
    {
        private DbEntityEntry entry;
        private AContext context;
        private Role model;

        [SetUp]
        public void SetUp()
        {
            context = new TestingContext();
            TearDownData();
            SetUpData();

            context = new TestingContext();
            entry = context.Entry(context.Set<Role>().SingleOrDefault());
            entry.State = EntityState.Modified;
        }

        [TearDown]
        public void TearDown()
        {
            context.Dispose();
        }

        #region Constructor: LoggableEntry(DbEntityEntry entry)

        [Test]
        public void LoggableEntry_SetsEntityBaseTypeThenEntityIsProxied()
        {
            if (entry.Entity.GetType().Namespace != "System.Data.Entity.DynamicProxies")
                Assert.Inconclusive();

            Type actual = new LoggableEntry(entry).EntityType;
            Type expected = model.GetType();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LoggableEntry_SetsEntityTypeThenEntityIsNotProxied()
        {
            entry = context.Entry(context.Set<Role>().Add(new Role()));
            if (entry.Entity.GetType().Namespace == "System.Data.Entity.DynamicProxies")
                Assert.Inconclusive();

            Type actual = new LoggableEntry(entry).EntityType;
            Type expected = model.GetType();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LoggableEntry_SetsState()
        {
            entry.State = EntityState.Deleted;

            String expected = entry.State.ToString().ToLower();
            String actual = new LoggableEntry(entry).State;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LoggableEntry_HasChangesIfAnyPropertyIsModified()
        {
            ((Role)entry.Entity).Name += "1";

            Assert.IsTrue(new LoggableEntry(entry).HasChanges);
        }

        [Test]
        public void LoggableEntry_DoNoHaveChangesIfPropertiesAreNotModified()
        {
            Assert.IsFalse(new LoggableEntry(entry).HasChanges);
        }

        [Test]
        public void LoggableEntry_CreatesProperties()
        {
            IEnumerable<String> properties = entry.CurrentValues.PropertyNames;

            IEnumerable<LoggablePropertyEntry> expected = properties.Select(name => new LoggablePropertyEntry(entry.Property(name)));
            IEnumerable<LoggablePropertyEntry> actual = new LoggableEntry(entry).Properties;

            TestHelper.EnumPropertyWiseEquals(expected, actual);
        }

        #endregion

        #region Method: ToString()

        [Test]
        public void ToString_FormsEntryRepresentation()
        {
            String actual = new LoggableEntry(entry).ToString();
            String expected = FormExpectedMessage(entry);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Test helpers

        private void SetUpData()
        {
            model = ObjectFactory.CreateRole();
            context.Set<Role>().Add(model);
            context.SaveChanges();
        }
        private void TearDownData()
        {
            context.Set<Role>().RemoveRange(context.Set<Role>());
            context.SaveChanges();
        }

        private String FormExpectedMessage(DbEntityEntry entry)
        {
            LoggableEntry loggableEntry = new LoggableEntry(entry);
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendFormat("{0} {1}:{2}",
                loggableEntry.EntityType.Name,
                loggableEntry.State,
                Environment.NewLine);

            foreach (LoggablePropertyEntry property in loggableEntry.Properties)
                messageBuilder.AppendFormat("    {0}{1}", property, Environment.NewLine);

            return messageBuilder.ToString();
        }

        #endregion
    }
}
