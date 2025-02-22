﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Dwapi.ExtractsManagement.Core.Interfaces.Extratcors.Cbs;
using Dwapi.ExtractsManagement.Core.Interfaces.Reader.Cbs;
using Dwapi.ExtractsManagement.Core.Interfaces.Repository.Cbs;
using Dwapi.ExtractsManagement.Core.Model.Destination.Cbs;
using Dwapi.ExtractsManagement.Core.Model.Source.Cbs;
using Dwapi.ExtractsManagement.Core.Notifications;
using Dwapi.ExtractsManagement.Core.Profiles;
using Dwapi.SharedKernel.Enum;
using Dwapi.SharedKernel.Events;
using Dwapi.SharedKernel.Model;
using Dwapi.SharedKernel.Utility;
using MediatR;
using Serilog;

namespace Dwapi.ExtractsManagement.Core.Extractors.Cbs
{
    public class MasterPatientIndexSourceExtractor : IMasterPatientIndexSourceExtractor
    {
        private readonly IMasterPatientIndexReader _reader;
        private readonly IMediator _mediator;
        private readonly ITempMasterPatientIndexRepository _extractRepository;

        public MasterPatientIndexSourceExtractor(IMasterPatientIndexReader reader, IMediator mediator, ITempMasterPatientIndexRepository extractRepository)
        {
            _reader = reader;
            _mediator = mediator;
            _extractRepository = extractRepository;
        }

        public async Task<int> Extract(DbExtract extract, DbProtocol dbProtocol)
        {
            var mapper = dbProtocol.SupportsDifferential ? ExtractDiffMapper.Instance : ExtractMapper.Instance;
            int batch = 500;

            DomainEvents.Dispatch(new CbsNotification(new ExtractProgress(nameof(MasterPatientIndex), "extracting...")));
            //DomainEvents.Dispatch(new CbsStatusNotification(extract.Id,ExtractStatus.Loading));

            var list = new List<TempMasterPatientIndex>();

            int count = 0;
            int totalCount = 0;

            using (var rdr = await _reader.ExecuteReader(dbProtocol, extract))
            {

                while (rdr.Read())
                {
                    totalCount++;
                    count++;
                    // AutoMapper profiles
                    var extractRecord = mapper.Map<IDataRecord, TempMasterPatientIndex>(rdr);
                    extractRecord.Id = LiveGuid.NewGuid();

                    if(!string.IsNullOrWhiteSpace(extractRecord.sxdmPKValueDoB))
                        list.Add(extractRecord);

                    if (count == batch)
                    {
                        // TODO: batch and save
                        _extractRepository.BatchInsert(list);

                        try
                        {
                            DomainEvents.Dispatch(new CbsNotification(new ExtractProgress(nameof(MasterPatientIndex), "extracting...",totalCount,count,0,0,0)));
                        }
                        catch (Exception e)
                        {
                            Log.Error(e,"Notification error");

                        }
                        count = 0;
                        list =new List<TempMasterPatientIndex>();
                    }

                    // TODO: Notify progress...


                }

                if (count > 0)
                {
                    _extractRepository.BatchInsert(list);
                }
                _extractRepository.CloseConnection();
            }

            try
            {

                DomainEvents.Dispatch(new CbsNotification(new ExtractProgress(nameof(MasterPatientIndex), "extracted", totalCount, 0, 0, 0, 0)));
                DomainEvents.Dispatch(new CbsStatusNotification(extract.Id, ExtractStatus.Found, totalCount));
                DomainEvents.Dispatch(new CbsStatusNotification(extract.Id, ExtractStatus.Loaded,totalCount));
            }
            catch (Exception e)
            {
                Log.Error(e, "Notification error");

            }

            return totalCount;
        }
    }
}
