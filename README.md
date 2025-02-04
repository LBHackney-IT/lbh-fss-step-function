This application and it's cloud recources has now been decommissioned

<!--
## Description
This repository contains code for a scheduled multi-step process of identifying **Find Support Services** organisations
in-need of re-verification, notifying them about it, and pausing them should their details not be verified after a certain
amount of time.

## Implementation
This solution consists of 2 parts:
1. The `startFunction` is deployed as a separate scheduled AWS lambda function that runs on loop once per specified time
interval. This function queries a list of organisations that should get flagged to enter the re-verification process,
and triggers a state machine (step function) for each such organisation.
2. The `fssstepfunc1` state machine consists of multiple AWS lambda functions that go through multiple steps in the business
process from flagging organisation for re-verification to pausing it if it hasn't been re-verified _(see **figure 1**)_.
Trigger time of each subsequent step is calculated by the step prior by adding `WAIT_DURATION` to current time.

<br/>

| <p align="center"><img src="https://github.com/user-attachments/assets/b65bb4d4-184d-4370-ae17-5484adbf3d69" /></p> |
| --- |
| **Figure 1.** The re-verification process step function <br/> run for each organisation separately. |

This solution has 2 external dependencies:
1. Find Support Services PostgreSQL database _(to query the organisations from)_.
2. GovUK Notify service _(to notify organisation managers/owners about the need for re-verification via email)_.

## Business
A lot of business details have been lost over the years, however, here a brief summary is presented.

### Organisations
Hackney has a lot of local organisations that offer advice, activities, and support to Hackney residents. These services
are grouped by various categories and can be looked up by the residents through Hackney's [public FSS front-end](https://find-support-services.hackney.gov.uk/).
The public FSS front-end is indexed by search engines _(**figure 2**)_, and shows service-providing organisation details and locations on the map _(**figure 3**)_. 

| ![image](https://github.com/user-attachments/assets/8e14451e-3778-4da4-8066-1541a8e11794) | ![image](https://github.com/user-attachments/assets/56dac25a-2705-4938-beeb-439b00eb9d66) |
| --- | --- |
| **Figure 2.** FSS is indexed and can be found via the web search engines. | **Figure 3.** Find Support Services organisation map. |

_(Front-end and back-end code for the public interface: [[FSS public FE](https://github.com/LBHackney-IT/lbh-fss-public-frontend)] , [FSS public API](https://github.com/LBHackney-IT/lbh-fss-public-api))_

### Management
Some of these services may be Council provided, however, others are provided by the 3rd party organisations.
Organisations can come and go, and additionally their contact details and services provided may change over time.

As such, Hackney has built a solution, where the organisation owners/managers can create accounts within the [FSS admin portal](https://find-support-services-admin.hackney.gov.uk/)
to manage their provided services and organisation details. Also, this is how new organisations can register to be searchable through FSS.
_(Github repositories: [[FSS portal API](https://github.com/LBHackney-IT/lbh-fss-portal-api)], [[FSS portal FE](https://github.com/LBHackney-IT/lbh-fss-portal-frontend)])_.

Hackney wants FSS system to have accurate and up-to-date information on services and organisations listed within. As such, any
new registering organisations have to undergo a review and verification process by the Hackney staff to be marked as ready to
show up within the FSS system and on the map.

### Re-verification
Additionally, any existing services have to be checked for whether their contact details are still up-to-date as they would be
used by the resident to contact the service for any inquiries, as well as used by the "[Better Conversations](https://frontdoor-snapshot.hackney.gov.uk/)"
_([repo](https://github.com/LBHackney-IT/coronavirus-frontdoor-snapshot))_ system to make resident referral to these services.

This is where **THIS REPOSITORY** comes along:
* According to the business requirements, organisations need to be checked for their contact and services details up-to-date'ness once a year.
* After a year passes since the organisation was last verified by the Hackney staff, it needs to be flagged into re-verification process.
* Once in re-verification, an organisation gets emailed about the need to verify whether their details are still correct within the admin system.
* If they do not log in and verify the details, after a few months a reminder will be sent.
* If nothing is done about it still, another reminder with a warning about the organisation's de-listing from FSS system is sent.
* If at any point the details have been re-verified, the organisation is flagged to have its usual status within FSS system.
* However, if details are not re-verified after that, the organisation gets flagged as paused and gets hidden from FSS search and map results.

This repository implements the find un-re-verified organisations, their status flagging, owner emailing, and organisation pausing logic
described above.

### Extra notes on re-verification
The exact duration for how much time has to pass between reminders has been lost to time, but it has been in 1-2 month range.

The re-verification by the organisation manager/owner is done by logging into the admin portal, clicking into special banner
notifying about the need of re-verification, and filling out the form (screenshots of the form can be see in [[FSS portal FE PR#70](https://github.com/LBHackney-IT/lbh-fss-portal-frontend/pull/70)].

There have also been talks regarding Hackney staff potentially double-checking & confirming re-verification as the last step
to the process, but I believe that this was either scrapped, or at least removed from the MVP scope.

## Final notes
* The work to implemt this feature has been completed, however, due to project's funding getting cut abruptly & leadership changing
the final approval for release was not given.
* A lof of time has passed since this was developed. For this application to get released, it would need to be upgraded from the now
deprecated `dotnetcore3.1` version to whichever runtime is being currently supported by the AWS lambda. Otherwise the deployment will fail.
* The AWS paramter store key from which the `WAIT_DURATION` variable is set to configure the duration between the reminder steps is likely
still set to debug values, which is minutes rather than days.
* For more information on Find Support Services, see this Google Drive folder [[link](https://drive.google.com/drive/folders/1r5vmflwvIVA3ZbEyg2QtNSzx21HswgTc)].

## Ownership
This along with the linked services and repositories is now owned by the [@LBHackney-IT/targeted-services](https://github.com/orgs/LBHackney-IT/teams/targeted-services) team.

-->