using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Xml.Linq;
using EnBiletBackend.Attributes;
using EnBiletBackend.Models;
using EnBiletBackend.Services;
using System;
using Microsoft.Extensions.Logging;

namespace EnBiletBackend.Controllers
{
    [ApiController]
    public class EventController : Controller
    {
        public EventService _eventService;
        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        //-------------VENUES--------------

        [HttpGet("getVenues")]
        [TheAuthorize]
        public List<VENUES> getVenues([FromQuery] string city)
        {
            return _eventService.getVenues(city);
        }

        [HttpGet("getTheVenue")]
        [TheAuthorize]
        public ActionResult<VENUES> GetTheVenue([FromQuery] int venueID)
        {
            var venue = _eventService.getTheVenue(venueID);

            if (venue == null)
                return NotFound(); // 404

            return Ok(venue); // 200
        }

        [HttpGet("getAllVenues")]
        [TheAuthorize]
        public List<VENUES> getAllVenues()
        {
            return _eventService.getAllVenues();
        }

        [HttpGet("getCities")]
        [TheAuthorize]
        public List<string> getCities()
        {
            return _eventService.getCities();
        }

        [HttpPost("AddVenue")]
        [TheAuthorize]
        public IActionResult AddVenue([FromBody] VENUES data)
        {
            var success = _eventService.AddVenue(data);

            if (!success)
                return Conflict(new { message = "Seçtiğiniz şehirde belirttiğiniz isimde bir salon zaten bulunuyor." });

            return Created("", new { message = "Venue created" });
        }

        [HttpPost("EditVenue")]
        [TheAuthorize]
        public IActionResult EditVenue([FromBody] VENUES data)
        {
            try
            {
                _eventService.EditVenue(data);
                return Ok(new { message = "Venue updated" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        //-------------EVENTS--------------

        [HttpGet("getEvents")]
        [TheAuthorize]
        public List<EVENTTYPE> getEvents()
        {
            return _eventService.getEvents();
        }

        [HttpGet("getTheEvent")]
        [TheAuthorize]
        public ActionResult<EVENTTYPE> getTheEvent([FromQuery] int eventID)
        {
            var venue = _eventService.getTheEvent(eventID);

            if (venue == null)
                return NotFound(); // 404

            return Ok(venue); // 200
        }

        [HttpPost("AddEvent")]
        [TheAuthorize]
        public IActionResult AddEvent([FromBody] ADDEVENTS data)
        {

            try
            {
                var eventID = _eventService.AddEvent(data);
                return Created("", new { eventID = eventID });
            }
            catch (InvalidOperationException)
            {
                return Conflict(new { message = "Seçtiğiniz tarihte ve salonda bir etkinlik zaten bulunuyor." });
            }



        }

        [HttpPost("EditEvent")]
        [TheAuthorize]
        public IActionResult EditEvent([FromBody] ADDEVENTS data)
        {
            try
            {
                _eventService.EditEvent(data);
                return Ok(new { message = "Event updated" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException)
            {
                return Conflict(new { message = "Seçtiğiniz tarihte ve salonda bir etkinlik zaten bulunuyor." });
            }
        }







        //-------------SHOWS--------------


        [HttpGet("getShows")]
        [TheAuthorize]
        public List<SHOWS> getShows()
        {
            return _eventService.getShows();
        }

        [HttpPost("AddShow")]
        [TheAuthorize]
        public IActionResult AddShow([FromBody] SHOWS data)
        {
            try
            {
                var showId = _eventService.AddShow(data);
                return Created("", new { showID = showId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = "Belirttiğiniz isimde bir gösteri zaten bulunuyor." });
            }
        }



        [HttpPost("EditShow")]
        [TheAuthorize]
        public IActionResult EditShow([FromBody] SHOWS data)
        {
            try
            {
                _eventService.EditShow(data);
                return Ok(new { message = "Show updated" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = "Belirttiğiniz isimde bir gösteri zaten bulunuyor." });
            }
        }


        //---------------SEAT MAPS---------------------

        [HttpGet("getMaps")]
        [TheAuthorize]
        public List<SEATMAPS> getMaps([FromQuery] int venueID)
        {
            return _eventService.getMaps(venueID);
        }

        [HttpPost("AddMap")]
        [TheAuthorize]
        public IActionResult AddMap([FromBody] SEATMAPS data)
        {
            try
            {
                var success = _eventService.AddMap(data);


                if (!success)
                    return Conflict(new { message = "Bu salonda bu isimde bir oturma planı zaten bulunuyor." });

                return Created("", new { message = "Map created" });

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }



        }

        [HttpPost("EditMap")]
        [TheAuthorize]
        public IActionResult EditMap([FromBody] SEATMAPS data)
        {
            try
            {
                _eventService.EditMap(data);
                return Ok(new { message = "Map updated" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }



        //---------------------EVENT SEATS -------------------------------



        [HttpPost("saveSeats")]
        [TheAuthorize]
        public async Task<IActionResult> SaveSeats([FromBody] SaveEventSeatsRequest request)
        {
            if (request.Seats == null || request.Seats.Count == 0)
                return BadRequest("No seats provided.");


            try
            {
                await _eventService.SaveEventSeatsAsync(request);
                return Ok(new { success = true });
            }

            


            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getEventSeats")]
        [TheAuthorize]
        public ActionResult<EVENTSEATS> getEventSeats([FromQuery] int eventID)
        {
            var venue = _eventService.getEventSeats(eventID);

            if (venue == null)
                return NotFound(); // 404

            return Ok(venue); // 200
        }
    }



}
