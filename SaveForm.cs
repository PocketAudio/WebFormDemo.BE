#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Data.SqlClient;

public static class Procedure
{
    // using a single form with an id of 1 for demo
    public static string DeleteForm = "DELETE FROM [dbo].[Forms] WHERE FormId = 1";
    public static string InsertFields = "INSERT INTO [dbo].[Forms] (FormId, Fields) VALUES (1, @fields)";
}

public static class Validation
{
    public static string FormIdMissing = "Field must belong to a form";
    public static string FieldPropTypeMissing = "Field must have a type";
    public static string FieldPropValueMissing = "Field must have a value";
    public static string FieldPropIdMissing = "Field identifier is required";
}


// my bad, forgot to change these to FieldId, FieldValue, FieldType
public class Field
{
    [JsonProperty("formId")]
    public int FormId { get; set; }

    [JsonProperty("fieldPropId")]
    public string FieldPropId { get; set; }

    [JsonProperty("fieldPropValue")]
    public object FieldPropValue { get; set; }

    [JsonProperty("fieldPropType")]
    public string FieldPropType { get; set; }
    
    [JsonProperty("fieldFriendlyName")]
    public string FieldFriendlyName { get; set; }
}


public static async Task<JsonResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation($"[Save Form] Request recieved");

    var connString = "removed for security..";

    try 
    {
        // deseralize
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var fields = JsonConvert.DeserializeObject<IEnumerable<Field>>(requestBody);

        // validate
        foreach (var field_ in fields)
        {
           
            if (field_.FieldPropId == "errors") // this was silly
                continue;

            if (field_.FormId <= 0)
                return new JsonResult(Validation.FormIdMissing);

            if (string.IsNullOrEmpty(field_.FieldPropId))
                return new JsonResult(Validation.FieldPropIdMissing);

            if (string.IsNullOrEmpty(field_.FieldPropType))
                return new JsonResult(Validation.FieldPropTypeMissing);
                
            if (field_.FieldPropValue == null)
                return new JsonResult(Validation.FieldPropValueMissing);
        }
        

        // persist
        using (var conn = new SqlConnection(connString))
        {
            conn.Open();

            using (var cmd = new SqlCommand(Procedure.DeleteForm, conn))
            {
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(Procedure.InsertFields, conn))
            {
                cmd.Parameters.AddWithValue("@fields", requestBody);
                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }

        // return fields
        return new JsonResult(fields);
    }
    catch(Exception ex)
    {
        // exception handling like this for sake of simplicity
        return new JsonResult("Unable to process request.");
    }
}
