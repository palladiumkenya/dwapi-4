﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dwapi.ExtractsManagement.Core.Interfaces.Extratcors.Cbs;
using Dwapi.ExtractsManagement.Core.Interfaces.Extratcors.Dwh;
using Dwapi.ExtractsManagement.Core.Interfaces.Utilities;
using Dwapi.ExtractsManagement.Core.Model.Destination.Dwh;
using Dwapi.ExtractsManagement.Core.Tests.TestArtifacts;
using Dwapi.ExtractsManagement.Infrastructure;
using Dwapi.SettingsManagement.Core.Model;
using Dwapi.SharedKernel.Model;
using Dwapi.SharedKernel.Utility;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dwapi.ExtractsManagement.Core.Tests.Extractors.Dwh
{
    [TestFixture]
    public class PatientPharmacySourceExtractorTests
    {
        private IPatientPharmacySourceExtractor _extractor;
        private List<Extract> _extracts;
        private DbProtocol _protocol;
        private ExtractsContext _extractsContext;

        [OneTimeSetUp]
        public void Init()
        {
            TestInitializer.ClearDb();
            TestInitializer.SeedData(TestData.GenerateEmrSystems(TestInitializer.EmrConnectionString));
            _protocol = TestInitializer.Protocol;
            _extracts = TestInitializer.Extracts.Where(x => x.DocketId.IsSameAs("NDWH")).ToList();
            _extractsContext = TestInitializer.ServiceProvider.GetService<ExtractsContext>();
        }

        [SetUp]
        public void SetUp()
        {
            _extractor = TestInitializer.ServiceProvider.GetService<IPatientPharmacySourceExtractor>();
        }

        [TestCase(nameof(PatientPharmacyExtract))]
        public void should_Extract(string name)
        {
            Assert.False(_extractsContext.TempMasterPatientIndices.Any());
            var extract = _extracts.First(x => x.Name.IsSameAs(name));
            var count = _extractor.Extract(extract, _protocol).Result;
            Assert.AreEqual(count,_extractsContext.TempMasterPatientIndices.Count());
            Console.WriteLine($"extracted {_extractsContext.TempMasterPatientIndices.Count()}");
        }
    }
}
