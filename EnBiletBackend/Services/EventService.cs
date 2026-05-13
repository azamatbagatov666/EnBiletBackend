using Crypt.Class;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using EnBiletBackend.Connection;
using EnBiletBackend.Models;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;


namespace EnBiletBackend.Services
{
    public class EventService
    {
        private readonly IDbConnection _connection;
        private readonly IConfiguration _config;



        public EventService(DapperContext context, IConfiguration config)
        {
            _connection = context.CreateConnection();
            _config = config;
        }


        //--------------------------VENUES--------------------------

        public List<VENUES> getVenues(string city)
        {


            var query = @"

            SELECT venueID, venueName from VENUES where city = @city order by venueName;

    ";

            return _connection.Query<VENUES>(query, new { city }).ToList();
        }


        public VENUES? getTheVenue(int venueID)
        {
            var query = @"
        SELECT *
        FROM VENUES
        WHERE venueID = @venueID;
    ";



            return _connection.QuerySingleOrDefault<VENUES>(query, new { venueID });

        }

        public List<VENUES> getAllVenues()
        {


            var query = @"

            SELECT venueID, venueName, city, address from VENUES order by city;

    ";

            return _connection.Query<VENUES>(query).ToList();
        }

        public List<string> getCities()
        {


            var query = @"

              SELECT distinct city from VENUES order by city;

    ";

            return _connection.Query<string>(query).ToList();
        }

        public bool AddVenue(VENUES data)
        {
            var query = @"INSERT INTO VENUES (city, venueName, address)
                  VALUES (@city, @venueName, @address)";

            try
            {
                var result = _connection.Execute(query, data);
                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Unique constraint violation
                return false;
            }
        }

        public void EditVenue(VENUES data)
        {
            if (string.IsNullOrWhiteSpace(data.venueName))
                throw new ArgumentException("Venue name cannot be empty");

            var query = @"UPDATE VENUES
                  SET venueName = @venueName, city = @city, address = @address
                  WHERE venueID = @venueID";

            var rows = _connection.Execute(query, data);

            if (rows == 0)
                throw new KeyNotFoundException("Venue not found");
        }



        //--------------------------EVENTS--------------------------


        public bool AddEvent(ADDEVENTS data)
        {
            var query = @"INSERT INTO EVENTS (venueID, showID, date)
                  VALUES (@venueID, @showID, @date)";

            try
            {
                var result = _connection.Execute(query, data);
                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Unique constraint violation
                return false;
            }
        }




        public List<EVENTTYPE> getEvents()
        {


            var query = @"

SELECT 
    e.eventID,
    e.showID,
    e.venueID,
    s.showName,
    FORMAT(e.[DATE], 'dd-MM-yyyy HH:mm') AS [Date], 
    v.[city], 
    v.[venueName], 
    e.[mapID],
    e.[ticketSale], 
    e.[isPublic],

    CONCAT(
        SUM(CASE WHEN es.status = 'sold' THEN 1 ELSE 0 END),
        '/',
        SUM(CASE WHEN es.status IN ('sold', 'available') THEN 1 ELSE 0 END)
    ) AS soldTickets

FROM [dbo].[EVENTS] e
INNER JOIN [dbo].[VENUES] v
    ON e.[venueID] = v.[venueID]
INNER JOIN [dbo].[SHOWS] s
    ON e.[showID] = s.[showID]
LEFT JOIN [dbo].[EVENT_SEATS] es
    ON es.eventID = e.eventID

GROUP BY
    e.eventID,
    e.showID,
    e.venueID,
    s.showName,
    e.[DATE],
    v.[city],
    v.[venueName],
    e.[mapID],
    e.[ticketSale],
    e.[isPublic];

                    ";

            return _connection.Query<EVENTTYPE>(query).ToList();
        }

        public EVENTTYPE? getTheEvent(int eventID)
        {
            var query = @"
  SELECT 
    e.eventID,
e.showID,
e.venueID,
    s.showName,
    FORMAT(e.[DATE], 'dd-MM-yyyy HH:mm') AS Date, 
    v.[city], 
    v.[venueName], 
    e.[ticketSale], 
    e.[mapID],
    e.[isPublic]
FROM [dbo].[EVENTS] e
INNER JOIN [dbo].[VENUES] v
    ON e.[venueID] = v.[venueID]
INNER JOIN [dbo].[SHOWS] s
    ON e.[showID] = s.[showID]
	
	WHERE e.eventID = @eventID ;
    ";



            return _connection.QuerySingleOrDefault<EVENTTYPE>(query, new { eventID });

        }


        public void EditEvent(ADDEVENTS data)
        {

            var seatCount = _connection.ExecuteScalar<int>(@"
    SELECT COUNT(*)
    FROM EVENT_SEATS
    WHERE eventID = @eventID
", new { data.eventID });

            if (data.ticketSale == true && seatCount == 0)
                throw new InvalidOperationException("Bilet fiyatlarını belirlemeden etkinliği satışa açamazsınız.");


            var query = @"UPDATE EVENTS
                  SET venueID = @venueID, showID = @showID, date = @date, ticketSale = @ticketSale, isPublic = @isPublic
                  WHERE eventID = @eventID";

            var rows = _connection.Execute(query, data);

            if (rows == 0)
                throw new KeyNotFoundException("Event not found");
        }





        //--------------------------SHOWS--------------------------






        public List<SHOWS> getShows()
        {


            var query = @"

                    SELECT showID, showName, description, imageKey, imageThumbKey from SHOWS ORDER BY createdAt DESC

                    ";

            return _connection.Query<SHOWS>(query).ToList();
        }

        public int AddShow(SHOWS data)
        {
            if (string.IsNullOrWhiteSpace(data.showName))
                throw new ArgumentException("Show name cannot be empty");

            var query = @"
        INSERT INTO SHOWS (showName, description, imageKey, imageThumbKey)
        OUTPUT INSERTED.showID
        VALUES (@showName, @description, @imageKey, @imageThumbKey)
    ";

            try
            {
                return _connection.ExecuteScalar<int>(query, data);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new InvalidOperationException();
            }
        }

        public void EditShow(SHOWS data)
        {
            if (string.IsNullOrWhiteSpace(data.showName))
                throw new ArgumentException("Show name cannot be empty");

            var query = @"UPDATE SHOWS
                  SET showName = @showName, description = @description, imageKey = @imageKey, imageThumbKey = @imageThumbKey
                  WHERE showID = @showID";

            

         


            try
            {
                var rows = _connection.Execute(query, data);
                if (rows == 0)
                    throw new KeyNotFoundException("Show not found");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new InvalidOperationException();
            }
        }



        //------------------------SEATMAPS-------------------------

        public List<SEATMAPS> getMaps(int venueID)
        {


            var query = @"

                    SELECT 
    s.mapID,
s.mapName,
s.venueID,
s.isSeated,
s.layoutJS,
s.maxCapacity,

    v.[venueName]

FROM [dbo].[SEAT_MAPS] s
INNER JOIN [dbo].[VENUES] v
    ON s.[venueID] = v.[venueID]
	where s.venueID = @venueID


                    ";

            return _connection.Query<SEATMAPS>(query, new { venueID }).ToList();
        }

        public bool AddMap(SEATMAPS data)
        {
            var query = @"INSERT INTO SEAT_MAPS (mapName, venueID, isSeated, layoutJS, maxCapacity)
                  VALUES (@mapName, @venueID, @isSeated, @layoutJS, @maxCapacity)";

            try
            {
                var result = _connection.Execute(query, data);
                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Unique constraint violation
                return false;
            }
        }


        public void EditMap(SEATMAPS data)
        {
            if (string.IsNullOrWhiteSpace(data.mapName))
                throw new ArgumentException("Map name cannot be empty");

            var isLocked = _connection.QuerySingleOrDefault<bool>(@"
        SELECT isLocked
        FROM vw_SeatMapLockStatus
        WHERE mapID = @mapID
    ", new { data.mapID });

            if (isLocked)
                throw new InvalidOperationException("Bu oturma planı bir etkinliğe atandığı için düzenlenemez.");

            var query = @"
        UPDATE SEAT_MAPS
        SET mapName = @mapName,
            layoutJS = @layoutJS,
            maxCapacity = @maxCapacity
        WHERE mapID = @mapID";

            var rows = _connection.Execute(query, data);

            if (rows == 0)
                throw new KeyNotFoundException("Plan bulunamadı.");
        }



        // --------------------- EVENT SEATS -------------------------------

        public async Task SaveEventSeatsAsync(SaveEventSeatsRequest request)
        {
            using var conn = _connection;

            if (conn.State != ConnectionState.Open)
                conn.Open(); // ✅ OPEN FIRST

            using var tx = conn.BeginTransaction(); // ✅ NOW safe

            try
            {

                var ticketSaleOn = await conn.ExecuteScalarAsync<bool>(@"
    SELECT ticketSale
    FROM EVENTS
    WHERE eventID = @eventID
", new { request.eventID }, tx);

            if (ticketSaleOn)
        
            throw new InvalidOperationException("Bilet satışı açıkken koltuk düzeni değiştirilemez.");


            var currentMapID = await conn.ExecuteScalarAsync<int?>(
    "SELECT mapID FROM EVENTS WHERE eventID = @eventID",
    new { request.eventID },
    tx
);

         
                // 1. block map change if sold seats exist AND map changes
                if (currentMapID == null || request.mapID != currentMapID)
                {
                    var hasSold = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM EVENT_SEATS
                WHERE eventID = @eventID AND status = 'sold'
            ", new { request.eventID }, tx);

                    if (hasSold > 0)
                    {
                        throw new Exception("Seatmap cannot be changed when seats are sold.");
                    }

                    // 2. delete old seats if map changes
                    await conn.ExecuteAsync(@"
                DELETE FROM EVENT_SEATS
                WHERE eventID = @eventID
            ", new { request.eventID }, tx);

                    // 3. update event map
                    await conn.ExecuteAsync(@"
                UPDATE EVENTS
                SET mapID = @mapID
                WHERE eventID = @eventID
            ", new { request.eventID, request.mapID }, tx);
                }

                // 4. upsert seats
                const string sql = @"
IF EXISTS (
    SELECT 1 FROM EVENT_SEATS
    WHERE eventID = @eventID AND cellID = @CellId
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM EVENT_SEATS
        WHERE eventID = @eventID
          AND cellID = @CellId
          AND status = 'sold'
          AND (price <> @Price OR status <> @Status)
    )
        THROW 50001, 'Sold seats cannot be modified', 1;

    UPDATE EVENT_SEATS
    SET price = @Price,
        status = @Status,
        updated_at = SYSUTCDATETIME()
    WHERE eventID = @eventID AND cellID = @CellId;
END
ELSE
BEGIN
    INSERT INTO EVENT_SEATS (eventID, cellID, price, status, created_at)
    VALUES (@eventID, @CellId, @Price, @Status, SYSUTCDATETIME());
END
";

                foreach (var seat in request.Seats)
                {
                    await conn.ExecuteAsync(sql, new
                    {
                        request.eventID,
                        CellId = seat.CellId,
                        Price = seat.Price,
                        Status = seat.Status
                    }, tx);
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<EVENTSEATS>? getEventSeats(int eventID)
        {
            var query = @"SELECT * FROM EVENT_SEATS WHERE eventID = @eventID ;
    ";




            return _connection.Query<EVENTSEATS>(query, new { eventID }).ToList();




        }


    }






}


