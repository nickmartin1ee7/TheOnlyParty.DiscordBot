using System.Text.Json.Serialization;

namespace TheOnlyParty.ClassLibrary.Flight;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public record Destination
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("code_icao")]
    public string? CodeIcao { get; set; }

    [JsonPropertyName("code_iata")]
    public string? CodeIata { get; set; }

    [JsonPropertyName("code_lid")]
    public string? CodeLid { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("airport_info_url")]
    public string? AirportInfoUrl { get; set; }
}

public record Flight
{
    [JsonPropertyName("ident")]
    public string? Ident { get; set; }

    [JsonPropertyName("ident_icao")]
    public string? IdentIcao { get; set; }

    [JsonPropertyName("ident_iata")]
    public string? IdentIata { get; set; }

    [JsonPropertyName("fa_flight_id")]
    public string? FaFlightId { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("operator_icao")]
    public string? OperatorIcao { get; set; }

    [JsonPropertyName("operator_iata")]
    public string? OperatorIata { get; set; }

    [JsonPropertyName("flight_number")]
    public string? FlightNumber { get; set; }

    [JsonPropertyName("registration")]
    public string? Registration { get; set; }

    [JsonPropertyName("atc_ident")]
    public string? AtcIdent { get; set; }

    [JsonPropertyName("inbound_fa_flight_id")]
    public string? InboundFaFlightId { get; set; }

    [JsonPropertyName("codeshares")]
    public List<string?> Codeshares { get; set; }

    [JsonPropertyName("codeshares_iata")]
    public List<string>? CodesharesIata { get; set; }

    [JsonPropertyName("blocked")]
    public bool? Blocked { get; set; }

    [JsonPropertyName("diverted")]
    public bool? Diverted { get; set; }

    [JsonPropertyName("cancelled")]
    public bool? Cancelled { get; set; }

    [JsonPropertyName("position_only")]
    public bool? PositionOnly { get; set; }

    [JsonPropertyName("origin")]
    public Origin? Origin { get; set; }

    [JsonPropertyName("destination")]
    public Destination? Destination { get; set; }

    [JsonPropertyName("departure_delay")]
    public int? DepartureDelay { get; set; }

    [JsonPropertyName("arrival_delay")]
    public int? ArrivalDelay { get; set; }

    [JsonPropertyName("filed_ete")]
    public int? FiledEte { get; set; }

    [JsonPropertyName("progress_percent")]
    public int? ProgressPercent { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("aircraft_type")]
    public string? AircraftType { get; set; }

    [JsonPropertyName("route_distance")]
    public int? RouteDistance { get; set; }

    [JsonPropertyName("filed_airspeed")]
    public int? FiledAirspeed { get; set; }

    [JsonPropertyName("filed_altitude")]
    public int? FiledAltitude { get; set; }

    [JsonPropertyName("route")]
    public string? Route { get; set; }

    [JsonPropertyName("baggage_claim")]
    public string? BaggageClaim { get; set; }

    [JsonPropertyName("seats_cabin_business")]
    public int? SeatsCabinBusiness { get; set; }

    [JsonPropertyName("seats_cabin_coach")]
    public int? SeatsCabinCoach { get; set; }

    [JsonPropertyName("seats_cabin_first")]
    public int? SeatsCabinFirst { get; set; }

    [JsonPropertyName("gate_origin")]
    public string? GateOrigin { get; set; }

    [JsonPropertyName("gate_destination")]
    public string? GateDestination { get; set; }

    [JsonPropertyName("terminal_origin")]
    public string? TerminalOrigin { get; set; }

    [JsonPropertyName("terminal_destination")]
    public string? TerminalDestination { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("scheduled_out")]
    public DateTime? ScheduledOut { get; set; }

    [JsonPropertyName("estimated_out")]
    public DateTime? EstimatedOut { get; set; }

    [JsonPropertyName("actual_out")]
    public DateTime? ActualOut { get; set; }

    [JsonPropertyName("scheduled_off")]
    public DateTime? ScheduledOff { get; set; }

    [JsonPropertyName("estimated_off")]
    public DateTime? EstimatedOff { get; set; }

    [JsonPropertyName("actual_off")]
    public DateTime? ActualOff { get; set; }

    [JsonPropertyName("scheduled_on")]
    public DateTime? ScheduledOn { get; set; }

    [JsonPropertyName("estimated_on")]
    public DateTime? EstimatedOn { get; set; }

    [JsonPropertyName("actual_on")]
    public DateTime? ActualOn { get; set; }

    [JsonPropertyName("scheduled_in")]
    public DateTime? ScheduledIn { get; set; }

    [JsonPropertyName("estimated_in")]
    public DateTime? EstimatedIn { get; set; }

    [JsonPropertyName("actual_in")]
    public DateTime? ActualIn { get; set; }

    [JsonPropertyName("foresight_predictions_available")]
    public bool? ForesightPredictionsAvailable { get; set; }
}

public record Links
{
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

public record Origin
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("code_icao")]
    public string? CodeIcao { get; set; }

    [JsonPropertyName("code_iata")]
    public string? CodeIata { get; set; }

    [JsonPropertyName("code_lid")]
    public string? CodeLid { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("airport_info_url")]
    public string? AirportInfoUrl { get; set; }
}

public record FlightsResult
{
    [JsonPropertyName("links")]
    public Links? Links { get; set; }

    [JsonPropertyName("num_pages")]
    public int? NumPages { get; set; }

    [JsonPropertyName("flights")]
    public List<Flight>? Flights { get; set; }
}

