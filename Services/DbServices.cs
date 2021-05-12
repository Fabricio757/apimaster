using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace WebApi.Services
{
    public interface IDbService
    {
        string Input(string input);
        string Get(string input);
        string Post(string input);
        string Put(string input);
    }

    public class DbService : IDbService
    {
        //private readonly AppSettings _appSettings;
        //private readonly IConfiguration _configuration;
        private readonly ConnectionString _connectionString;

        //public DbService(IOptions<AppSettings> appSettings)
        public DbService(IOptions<ConnectionString> connectionString)
        {
            _connectionString = connectionString.Value;
        }

        public string Input(string input){
            try{
                string JsonResponse = "{\"resultados\": [";
                JObject jInput = JObject.Parse(input);
                string jBaseDatos = jInput["BaseDatos"].ToString();
                
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString.AnimalsDB))
                {
                    sqlConnection.Open();
                    JArray jOperaciones = (JArray)jInput["Operaciones"];
                    int i = 1;
                    foreach (JObject jOperacion in jOperaciones)
                    {
                        string jOp = jOperacion["Op"].ToString();
                        if (jOp == "Insert")
                        {
                            string jTabla = jOperacion["tabla"].ToString();
                            string qryInsert = "INSERT INTO " + jTabla + " ";

                            JArray jArgs = (JArray)jOperacion["args"];
                            bool first = true;
                            string qryInsertValues = ") values (";
                            foreach (JObject jArg in jArgs)
                            {
                                JProperty property = jArg.Properties().ToList()[0];
                                string jType = property.Name.Split(':')[1];
                                string jName = property.Name.Split(':')[0];
                                qryInsert += (first ? " (" : ", ") + jName;
                                qryInsertValues += (first ? "" : ", ") + FormatValue(jType, property.Value.ToString());
                                first = false;
                            }
                            qryInsert += qryInsertValues + ")";

                            SqlCommand sqlCommand = new SqlCommand(qryInsert, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{'" + i.ToString() + "': " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                        if (jOp == "Seleccion")
                        {
                            string jTabla = jOperacion["tabla"].ToString();
                            string qrySelect = "SELECT * FROM " + jTabla;

                            JArray jArgs = (JArray)jOperacion["args"];
                            if (!(jArgs is null)){
                                qrySelect += (jArgs.Count > 0  ? " WHERE " : "");
                                foreach (JObject jArg in jArgs)
                                {
                                    JProperty property = jArg.Properties().ToList()[0];
                                    qrySelect += property.Name + " = " + property.Value;
                                }
                            }
                            SqlCommand sqlCommand = new SqlCommand(qrySelect, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{\"" + i.ToString() + "\": " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                        if (jOp == "Delete")
                        {
                            string jTabla = jOperacion["tabla"].ToString();
                            string qrySelect = "DELETE FROM " + jTabla;

                            JArray jArgs = (JArray)jOperacion["args"];
                            if (!(jArgs is null)){
                                qrySelect += (jArgs.Count > 0  ? " WHERE " : "");
                                foreach (JObject jArg in jArgs)
                                {
                                    JProperty property = jArg.Properties().ToList()[0];
                                    qrySelect += property.Name + " = " + property.Value;
                                }
                            }
                            SqlCommand sqlCommand = new SqlCommand(qrySelect, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{\"" + i.ToString() + "\": " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                        if (jOp == "Update")
                        {
                            string jTabla = jOperacion["tabla"].ToString();
                            string qryUpdate = "UPDATE " + jTabla;

                            JArray jArgs = (JArray)jOperacion["args"];
                            bool firstWhere = true;
                            bool firstSet = true;

                            string qrySET = " SET ";

                            string qryWHERE = " WHERE ";
                            foreach (JObject jArg in jArgs)
                            {
                                JProperty property = jArg.Properties().ToList()[0];
                                string jType = property.Name.Split(':')[1];
                                string jName = property.Name.Split(':')[0];

                                JProperty property1 = jArg.Properties().ToList()[1];
                                string jData = property1.Value.ToString();
                        
                                if(jData == "key")
                                {
                                    qryWHERE += (firstWhere ? " " : " and ") + jName + " = " + FormatValue(jType, property.Value.ToString());                                    
                                    firstWhere = false;
                                }
                                else
                                {
                                    qrySET += (firstSet ? " " : " and ") + jName + " = " + FormatValue(jType, property.Value.ToString());
                                    firstSet = false;
                                }
                            }
                            qryUpdate += qrySET + " " + qryWHERE;

                            SqlCommand sqlCommand = new SqlCommand(qryUpdate, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{'" + i.ToString() + "': " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                    }
                    sqlConnection.Close();
                }
                JsonResponse += "]}";
                //JsonResponse = "{'resultados': [{'1': [{'id':1,'nombre':'Perro     '},{'id':2,'nombre':'Gato      '},{'id':3,'nombre':'Leon      '},{'id':4,'nombre':'Tigre     '},{'id':5,'nombre':'Chita     '},{'id':7,'nombre':'Orca      '},{'id':8,'nombre':'Delfin    '}]}";
                return JsonResponse;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public string Get(string input){
            try{
                string JsonResponse = "{'resultados': [";
                JObject jInput = JObject.Parse(input);
                string jBaseDatos = jInput["BaseDatos"].ToString();
                
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString.AnimalsDB))
                {
                    sqlConnection.Open();
                    JArray jOperaciones = (JArray)jInput["Operaciones"];
                    int i = 1;
                    foreach (JObject jSelect in jOperaciones)
                    {
                        string jOp = jSelect["Op"].ToString();
                        if (jOp == "Seleccion")
                        {
                            string jTabla = jSelect["tabla"].ToString();
                            string qrySelect = "SELECT * FROM " + jTabla;

                            JArray jArgs = (JArray)jSelect["args"];
                            qrySelect += (jArgs.Count > 0  ? " WHERE " : "");
                            foreach (JObject jArg in jArgs)
                            {
                                JProperty property = jArg.Properties().ToList()[0];
                                qrySelect += property.Name + " = " + property.Value;
                            }
                            SqlCommand sqlCommand = new SqlCommand(qrySelect, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{'" + i.ToString() + "': " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                    }
                    sqlConnection.Close();
                }
                JsonResponse += "]}";
                return JsonResponse;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        public string Post(string input){
            try{
                string JsonResponse = "{'resultados': [";
                JObject jInput = JObject.Parse(input);
                string jBaseDatos = jInput["BaseDatos"].ToString();
                
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString.AnimalsDB))
                {
                    sqlConnection.Open();
                    JArray jOperaciones = (JArray)jInput["Operaciones"];
                    int i = 1;
                    foreach (JObject jOperacion in jOperaciones)
                    {
                        string jOp = jOperacion["Op"].ToString();
                        if (jOp == "Insert")
                        {
                            string jTabla = jOperacion["tabla"].ToString();
                            string qryInsert = "INSERT INTO " + jTabla + " ";

                            JArray jArgs = (JArray)jOperacion["args"];
                            bool first = true;
                            string qryInsertValues = ") values (";
                            foreach (JObject jArg in jArgs)
                            {
                                JProperty property = jArg.Properties().ToList()[0];
                                string jType = property.Name.Split(':')[1];
                                string jName = property.Name.Split(':')[0];
                                qryInsert += (first ? " (" : ", ") + jName;
                                qryInsertValues += (first ? "" : ", ") + FormatValue(jType, property.Value.ToString());
                                first = false;
                            }
                            qryInsert += qryInsertValues + ")";

                            SqlCommand sqlCommand = new SqlCommand(qryInsert, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{'" + i.ToString() + "': " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                    }
                    sqlConnection.Close();
                }
                JsonResponse += "]}";
                return JsonResponse;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        private string FormatValue(string jType, string jValue)
        {
            switch (jType)
            { 
            case "string": return "'" + jValue + "'"; 
            default: return jValue;
            }
        }
        public string Put(string input){
            try{
                string JsonResponse = "{'resultados': [";
                JObject jInput = JObject.Parse(input);
                string jBaseDatos = jInput["BaseDatos"].ToString();
                
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString.AnimalsDB))
                {
                    sqlConnection.Open();
                    JArray jOperaciones = (JArray)jInput["Operaciones"];
                    int i = 1;
                    foreach (JObject jOperacion in jOperaciones)
                    {
                        string jOp = jOperacion["Op"].ToString();
                        if (jOp == "Update")
                        {
                            string jTabla = jOperacion["tabla"].ToString();
                            string qryUpdate = "UPDATE " + jTabla;

                            JArray jArgs = (JArray)jOperacion["args"];
                            bool firstWhere = true;
                            bool firstSet = true;

                            string qrySET = " SET ";

                            string qryWHERE = " WHERE ";
                            foreach (JObject jArg in jArgs)
                            {
                                JProperty property = jArg.Properties().ToList()[0];
                                string jType = property.Name.Split(':')[1];
                                string jName = property.Name.Split(':')[0];

                                JProperty property1 = jArg.Properties().ToList()[1];
                                string jData = property1.Value.ToString();
                        
                                if(jData == "key")
                                {
                                    qryWHERE += (firstWhere ? " " : " and ") + jName + " = " + FormatValue(jType, property.Value.ToString());                                    
                                    firstWhere = false;
                                }
                                else
                                {
                                    qrySET += (firstSet ? " " : " and ") + jName + " = " + FormatValue(jType, property.Value.ToString());
                                    firstSet = false;
                                }
                            }
                            qryUpdate += qrySET + " " + qryWHERE;

                            SqlCommand sqlCommand = new SqlCommand(qryUpdate, sqlConnection);
                            SqlDataReader dr = sqlCommand.ExecuteReader();
                            var datatable = new DataTable();
                            datatable.Load(dr);
                            JsonResponse += "{'" + i.ToString() + "': " + JsonConvert.SerializeObject(datatable) + "}";
                            i++;
                        }
                    }
                    sqlConnection.Close();
                }
                JsonResponse += "]}";
                return JsonResponse;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }


    }
}