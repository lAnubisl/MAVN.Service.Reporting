﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using MAVN.Service.Reporting.Domain.Services;
using MAVN.Service.Reporting.Client.Api;
using MAVN.Service.Reporting.Client.Models;
using MAVN.Service.Reporting.DomainServices.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.Reporting.Controllers
{
    [ApiController]
    [Route("api")]
    public class TransactionReportController : ControllerBase, IReportApi
    {
        private readonly ITransactionReportReader _reportReader;
        private readonly IMapper _mapper;

        public TransactionReportController(
            ITransactionReportReader reportReader,
            IMapper mapper)
        {
            _reportReader = reportReader;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of transaction infos
        /// </summary>
        /// <param name="pagingInfo">The paging information</param>
        /// <param name="partnerIds"></param>
        /// <param name="transactionType">Optional</param>
        /// <param name="status">Optional</param>
        /// <returns></returns>
        [HttpGet("report")]
        [ProducesResponseType(typeof(PaginatedReportResult), (int)HttpStatusCode.OK)]
        public async Task<PaginatedReportResult> FetchReportAsync(
            [FromQuery] TransactionReportByTimeRequest pagingInfo,
            [FromQuery] string[] partnerIds,
            [FromQuery] string transactionType = null,
            [FromQuery] string status = null)
        {
            var result = await _reportReader.GetPaginatedAsync(
                pagingInfo.CurrentPage, pagingInfo.PageSize,
                pagingInfo.From, pagingInfo.To, partnerIds,
                transactionType, status);

            return _mapper.Map<PaginatedReportResult>(result);
        }

        /// <summary>
        /// Get Csv file of transaction infos
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="partnerIds"></param>
        /// <param name="transactionType">Optional</param>
        /// <param name="status">Optional</param>
        /// <returns></returns>
        [HttpGet("report/csv")]
        [ProducesResponseType(typeof(CsvFileReportResult), (int)HttpStatusCode.OK)]
        public async Task<CsvFileReportResult> FetchReportCsvAsync(
            [FromQuery] [Required] DateTime from,
            [FromQuery] [Required] DateTime to,
            [FromQuery] string[] partnerIds,
            [FromQuery] string transactionType = null,
            [FromQuery] string status = null)
        {
            var reports = await _reportReader.GetLimitedAsync(
                from, to, Constants.LimitOfReports,
                partnerIds, transactionType, status);
            var result = CsvConverter.Run(reports);

            return new CsvFileReportResult
            {
                Content = result.ToUtf8Bytes()
            };
        }
    }
}
