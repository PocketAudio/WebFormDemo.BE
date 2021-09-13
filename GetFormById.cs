#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Data.SqlClient;

public static class Procedure
{
    // using a single form with an id of 1 for demo
    public static string GetForm = "SELECT * FROM [dbo].[Forms] WHERE FormId = 1";
}


public static async Task<JsonResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation($"[Get Form By Id] Request recieved");

    // not used for demo.. but the client would send the id of the form we want to return
    string id = req.Query["id"];

    var connString = "removed for security..";

    // should return data object (IEnumerable<Field>) but for the sake of simplicity 
    // returning the json string (we are pretty safe here since any records are checked for deserialization before inserted)
    var resultFields = String.Empty;

    try
    {
        using (var conn = new SqlConnection(connString))
        {
            conn.Open();

            using (var cmd = new SqlCommand(Procedure.GetForm, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        resultFields = reader["Fields"].ToString();
                    }
                }
            }
        }

        // this was silly
        return new JsonResult(resultFields);
    }
    catch(Exception ex)
    {
        // exception handling like this for sake of simplicity
        return new JsonResult("Unable to process request.");
    }
}
