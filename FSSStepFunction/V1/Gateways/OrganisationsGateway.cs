using System;
using System.Collections.Generic;
using System.Linq;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LbhFssStepFunction.V1.Gateways
{
    public class OrganisationsGateway : IOrganisationGateway
    {
        private readonly DatabaseContext _context;

        public OrganisationsGateway()
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            LoggingHandler.LogInfo($"Connection string: {connectionString.Substring(0,10)}...");
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(connectionString);
            _context = new DatabaseContext(optionsBuilder.Options);
        }


        public OrganisationDomain GetOrganisationById(int id)
        {
            try
            {
                LoggingHandler.LogInfo("Initiating database query");
                var organisation = _context.Organisations
                    .Include(o => o.UserOrganisations)
                    .ThenInclude(uo => uo.User)
                    .FirstOrDefault(o => o.Id == id);
                return organisation.ToDomain();
            }
            catch (Exception e)
            {
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
                throw;
            }
        }
    }
}