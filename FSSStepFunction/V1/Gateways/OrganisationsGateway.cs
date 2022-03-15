using System;
using System.Collections.Generic;
using System.Linq;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Errors;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LbhFssStepFunction.V1.Gateways
{
    public class OrganisationsGateway : IOrganisationsGateway
    {
        private readonly DatabaseContext _context;

        public OrganisationsGateway(string connectionString = null)
        {
            var _connectionString = connectionString ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(_connectionString);
            _context = new DatabaseContext(optionsBuilder.Options);
        }

        public OrganisationDomain GetOrganisationById(int id)
        {
            try
            {
                LoggingHandler.LogInfo($"Initiating database query for organisation with id {id}");
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

        public List<OrganisationDomain> GetOrganisationsToReview()
        {
            var orgsToReview = _context.Organisations
                .Where(x => x.LastRevalidation < DateTime.Today.AddDays(-365))
                .Where(x => !x.InRevalidationProcess)
                .Where(x => x.Status.ToLower() == "published")
                .Select(org => org.ToDomain())
                .ToList();
            return orgsToReview;
        }

        public OrganisationDomain PauseOrganisation(int id)
        {
            var orgToPause = _context.Organisations.Find(id);
            orgToPause.Status = "Paused";
            _context.Organisations.Attach(orgToPause);
            _context.SaveChanges();
            var org = _context.Organisations.Find(id);
            return org.ToDomain();
        }

        public void FlagOrganisationToBeInRevalidation(int id)
        {
            try {
                LoggingHandler.LogInfo($"Attempting to put organisation with id={id} to re-validation process");
                
                var organistionToFlag = _context.Organisations.Find(id);

                if (organistionToFlag == null)
                    throw new ResourceNotFoundException($"Organisation with id={id} was not found.");
                
                organistionToFlag.InRevalidationProcess = true;
                _context.SaveChanges();
            }
            catch (Exception ex) {
                LoggingHandler.LogError($"Failure while flagging organisation with id={id} as in re-validation process!");
                LoggingHandler.LogError(ex.Message);
                LoggingHandler.LogError(ex.InnerException?.Message);
                LoggingHandler.LogError(ex.StackTrace);
                throw; // terminate
            }
        }
    }
}