﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dwapi.ExtractsManagement.Core.Interfaces.Source.Psmart.Reader;
using Dwapi.ExtractsManagement.Infrastructure.Source.Psmart.Reader;
using Dwapi.SharedKernel.Enum;
using Dwapi.SharedKernel.Model;
using NUnit.Framework;

namespace Dwapi.ExtractsManagement.Infrastructure.Tests.Source.Psmart
{
    [TestFixture]
    public class PsmartSourceReaderTests
    {
        private IPsmartSourceReader _psmartSourceReader;
        private DbProtocol _mssql, _mysql;
        private DbExtract _extractA, _extractB;

        [SetUp]
        public void SetUp()
        {
           _mssql = new DbProtocol(DatabaseType.MicrosoftSQL, @".\koske14", "sa", "maun", "IQTools_KeHMIS");
            _extractA = new DbExtract {ExtractSql = @" SELECT [Serial],[Demographics],[Encounters] FROM [psmart]",Emr = "IQCare"};
            _mysql = new DbProtocol(DatabaseType.MySQL, @"localhost", "root", "root", "testemr");
            _extractB = new DbExtract { ExtractSql = @" select serial,demographics,encounters FROM psmart",Emr = "KenyaEMR"};
            _psmartSourceReader = new PsmartSourceReader();
        }

        [Test]
        public void should_Read_Psmart_MSSQL()
        {
            var psmartSources = _psmartSourceReader.Read(_mssql, _extractA).ToList();
            Assert.True(psmartSources.Count>0);
            Console.WriteLine(_mssql);
            foreach (var psmartSource in psmartSources)
            {
                Console.WriteLine(psmartSource);
            }
        }

        [Test]
        public void should_Read_Psmart_MySQL()
        {
            var psmartSources = _psmartSourceReader.Read(_mysql, _extractB).ToList();
            Assert.True(psmartSources.Count > 0);
            Console.WriteLine(_mysql);
            foreach (var psmartSource in psmartSources)
            {
                Console.WriteLine(psmartSource);
            }
        }

    }
}