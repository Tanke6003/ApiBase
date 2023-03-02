using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Utilities;
using System.Data.SqlClient;
using System.Data;
using Models;
using Utilities;
namespace Jabil.Data
{
    /// <summary>Provides methods to interact with SQL Sever databases.</summary>
    public class cCnx
    {
        #region [Variables]
        private string vCn;
        private int vTo;
        public List<SqlParameter> lstParameters = new List<SqlParameter>();
        public struct estParametres
        {
            /// <summary>
            /// Recibe el nombre del parametro del Store Procedure
            /// <example><para>Ejemplo: "@Nombre"</para></example>
            /// </summary>
            public string vName;
            /// <summary>
            /// Recibe el valor del parametro
            /// <example><para>Ejemplo: "Alfredo Rodriguez"</para></example>
            /// </summary>
            public string vValue;
        }
        public estParametres[] arrParametres = new estParametres[20];
        /// <summary>Gets the connection value from ConfigurationManager.ConnectionStrings</summary>
        public string ConnectionString
        {
            get
            {
                try
                {
                    return vCn;
                }
                catch { return string.Empty; }
            }
        }

        /// <summary>Sets the timeout of the SQL methods.</summary>
        public int SetSQLTimeout { set { vTo = value; } }

        /// <summary>Gets the name or network address of the instance fo SQL Server to connect to.</summary>
        public string DataSource { get { try { return this.fnGetConnectionStringParts().DataSource; } catch { return null; } } }

        /// <summary>Gets the name of the database associed with the connection.</summary>
        public string InitialCatalog { get { try { return this.fnGetConnectionStringParts().InitialCatalog; } catch { return null; } } }

        /// <summary>Gets a Boolean value that indicates if security-sensitive information, such as the password, is not returned as part of the connection if the connection is open or has ever been in an open state.</summary>
        public bool PersistSecurityInfo { get { try { return this.fnGetConnectionStringParts().PersistSecurityInfo; } catch { return false; } } }

        /// <summary>Sets the user ID to be used when connecting to SQL Server.</summary>
        public string UserID { get { try { return this.fnGetConnectionStringParts().UserID; } catch { return null; } } }

        /// <summary>Gets the password for the SQL Server account.</summary>
        public string Password { get { try { return this.fnGetConnectionStringParts().Password; } catch { return null; } } }
        #endregion

        #region [Constructors]
        /// <summary>Provides methods to interactuate with SQL Sever.</summary>
        public cCnx() { }

        public cCnx(string Seccion)
        {
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
            vCn = "Data Source=" +
            decryptProperty(builder.Configuration.GetSection(Seccion + ":Server").Value.ToString(), builder.Configuration.GetSection("AppSettings:Appkeys").Value.ToString()) +
            ";Initial Catalog=" +
            decryptProperty(builder.Configuration.GetSection(Seccion + ":Catalog").Value.ToString(), builder.Configuration.GetSection("AppSettings:Appkeys").Value.ToString()) +
            ";Persist Security Info=True;User ID=" +
            decryptProperty(builder.Configuration.GetSection(Seccion + ":User").Value.ToString(), builder.Configuration.GetSection("AppSettings:Appkeys").Value.ToString()) +
            ";Password=" +
            decryptProperty(builder.Configuration.GetSection(Seccion + ":Password").Value.ToString(), builder.Configuration.GetSection("AppSettings:Appkeys").Value.ToString());
        }
        public cCnx(string Server, string DataBase, string User, string Password)
        {
            vCn = "Data Source=" + Server + ";Initial Catalog=" + DataBase + ";Persist Security Info=True;User ID=" + User + ";Password=" + Password;
        }
        private string decryptProperty(string property, string key)
        {
            try
            {
                EncryptStr encryptStr = new EncryptStr();
                encryptStr.key = key;
                encryptStr.str = property;
                encryptStr = MultiUtilities.Decrypt(encryptStr);
                return encryptStr.result;

            }
            catch (System.Exception)
            {

                return "";
            }


        }

        #endregion

        #region [SQL Methods]
        /// <summary>This method copies all rows in the supplied System.Data.DataTable in the specified destination table on the server.</summary>
        /// <param name="DestinationTableName">Name of the destination table on the server.</param>
        /// <param name="DataTableToUpload">System.Data.DataTable to write in the destination table.</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        public void BulkCopy(string DestinationTableName, System.Data.DataTable DataTableToUpload, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;

                if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.Exception: Connection string is null.");

                var sqlBulkCopy = new System.Data.SqlClient.SqlBulkCopy(this.ConnectionString);

                foreach (System.Data.DataColumn Column in DataTableToUpload.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(Column.ColumnName, Column.ColumnName);
                }

                sqlBulkCopy.DestinationTableName = DestinationTableName;
                sqlBulkCopy.BulkCopyTimeout = (vTo > 0) ? vTo : 90;
                sqlBulkCopy.WriteToServer(DataTableToUpload);
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
            }
            finally { }
        }

        /// <summary>This method execute a SQL command and returns a DataTable as result.</summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        /// <returns>System.Data.DataTable</returns>
        public System.Data.DataTable GetDataTable(string cmdText, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;

                if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.Exception: Connection string is null.");

                var oDt = new System.Data.DataTable();
                var oCn = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
                var oCmd = new System.Data.SqlClient.SqlCommand(cmdText, oCn);
                oCmd.CommandTimeout = (vTo > 0) ? vTo : 90;
                var oDA = new System.Data.SqlClient.SqlDataAdapter(oCmd);
                oDA.Fill(oDt);

                return oDt;
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                return new System.Data.DataTable();
            }
            finally { }
        }

        /// <summary>This method execute a SQL command and returns a DataTable collection as result.</summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        /// <returns>System.Data.DataSet</returns>
        public System.Data.DataSet GetDataSet(string cmdText, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;

                if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.Exception: Connection string is null.");

                var oDs = new System.Data.DataSet();
                var oCn = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
                var oCmd = new System.Data.SqlClient.SqlCommand(cmdText, oCn);
                oCmd.CommandTimeout = (vTo > 0) ? vTo : 90;
                var oDA = new System.Data.SqlClient.SqlDataAdapter(oCmd);
                oDA.Fill(oDs);

                return oDs;
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                return new System.Data.DataSet();
            }
            finally { }
        }

        /// <summary>This method exectute a SQL command but it doesn't returns any result.</summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        public void Exec(string cmdText, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;

                if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.Exception: Connection string is null.");

                var oCn = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
                oCn.Open();
                var oCmd = new System.Data.SqlClient.SqlCommand(cmdText, oCn);
                oCmd.CommandTimeout = (vTo > 0) ? vTo : 90;
                oCmd.ExecuteNonQuery();
                oCn.Close();
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
            }
            finally { }
        }

        /// <summary>This method execute a SQL stored procedure.</summary>
        /// <param name="object_id">Name of the stored procedure name.</param>
        /// <param name="args">An object array that contains the parameters of the stored procedure.</param>
        public void Exec(string object_id, params object[] args)
        {
            string ExceptionMessage = string.Empty;

            if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.Exec.Exception: Connection string is null.");

            var cmdText = fnStoredProcedureCommandBuilder(out ExceptionMessage, object_id, args);

            if (ExceptionMessage.Length > 0) throw new Exception(ExceptionMessage);

            var oCn = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
            oCn.Open();
            var oCmd = new System.Data.SqlClient.SqlCommand(cmdText, oCn);
            oCmd.CommandTimeout = (vTo > 0) ? vTo : 90;
            oCmd.ExecuteNonQuery();
            oCn.Close();
        }

        /// <summary>This method execute a SQL stored procedure and provides a System.DataTable.</summary>
        /// <param name="object_id">Name of the stored procedure name.</param>
        /// <param name="args">An object array that contains the parameters of the stored procedure.</param>
        /// <returns>System.Data.DataTable</returns>
        public System.Data.DataTable ExecDataTable(string object_id, params object[] args)
        {
            string ExceptionMessage = string.Empty;

            if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.ExecDataTable.Exception: Connection string is null.");

            var VCmdText = fnStoredProcedureCommandBuilder(out ExceptionMessage, object_id, args);

            if (ExceptionMessage.Length > 0) throw new Exception(ExceptionMessage);

            var ODataSource = new System.Data.DataTable();
            var OCn = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
            var OCmd = new System.Data.SqlClient.SqlCommand(VCmdText, OCn);
            OCmd.CommandTimeout = (vTo > 0) ? vTo : 90;
            var ODA = new System.Data.SqlClient.SqlDataAdapter(OCmd);
            ODA.Fill(ODataSource);

            return ODataSource;
        }

        /// <summary>This method execute a SQL stored procedure and provides a System.DataSet.</summary>
        /// <param name="object_id">Name of the stored procedure name.</param>
        /// <param name="args">An object array that contains the parameters of the stored procedure.</param>
        /// <returns>System.Data.DataSet</returns>
        public System.Data.DataSet ExecDataSet(string object_id, params object[] args)
        {
            string ExceptionMessage = string.Empty;

            if (vCn == null || (vCn != null && vCn.Length.Equals(0))) throw new Exception("Jabil.Data.ExecDataSet.Exception: Connection string is null.");

            var VCmdText = fnStoredProcedureCommandBuilder(out ExceptionMessage, object_id, args);

            if (ExceptionMessage.Length > 0) throw new Exception(ExceptionMessage);

            var ODataSource = new System.Data.DataSet();
            var OCn = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
            var OCmd = new System.Data.SqlClient.SqlCommand(VCmdText, OCn);
            OCmd.CommandTimeout = (vTo > 0) ? vTo : 90;
            var ODA = new System.Data.SqlClient.SqlDataAdapter(OCmd);
            ODA.Fill(ODataSource);

            return ODataSource;
        }
        #endregion

        #region [Private Methods]
        /// <summary></summary>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        /// <param name="object_id">Name of the stored procedure name.</param>
        /// <param name="args">An object array that contains the parameters of the stored procedure.</param>
        /// <returns>System.String</returns>
        private string fnStoredProcedureCommandBuilder(out string ExceptionMessage, string object_id, params object[] args)
        {
            try
            {
                ExceptionMessage = string.Empty;
                string cmdText = string.Concat("EXEC ", object_id);

                if (args != null && args.Length > 0)
                {

                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == null)
                        {
                            cmdText = string.Concat(cmdText, " null", ((i < (args.Length - 1)) ? "," : string.Empty));
                        }
                        else
                        {
                            string type = args[i].GetType().ToString().ToLower();
                            Type OType = args[i].GetType();

                            if (type.EndsWith("char") || type.EndsWith("string"))
                                cmdText = string.Concat(cmdText, " '", (string)args[i], "'", ((i < (args.Length - 1)) ? "," : string.Empty));
                            else if (type.EndsWith("datetime"))
                                cmdText = string.Concat(cmdText, " '", ((DateTime)args[i]).ToString("yyyy-MM-dd HH:mm:ss"), "'", ((i < (args.Length - 1)) ? "," : string.Empty));
                            else
                                cmdText = string.Concat(cmdText, " ", args[i], ((i < (args.Length - 1)) ? "," : string.Empty));
                        }
                    }
                }

                return cmdText;
            }
            catch (Exception ex)
            {
                ExceptionMessage = string.Concat("Jabil.Source.fnStoredProcedureCommandBuilder.Exception: ", ex.Message);
                return string.Empty;
            }
        }

        private System.Data.SqlClient.SqlConnectionStringBuilder fnGetConnectionStringParts()
        {
            try
            {
                System.Data.SqlClient.SqlConnectionStringBuilder OSqlConnectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(this.ConnectionString);
                return OSqlConnectionStringBuilder;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region [File Methods]
        /// <summary>This method creates a File and saves it in the specified path.</summary>
        /// <param name="FileName">Represents the complete path with the file name and extension. Example: "C:\Files\File.csv"</param>
        /// <param name="FileData">Represents the file content encoded in a bytes array. </param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        public void SaveFile(string FileName, Byte[] FileData, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;
                using (var fs = new System.IO.FileStream(FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    fs.Write(FileData, 0, FileData.Length);
                }
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
            }
            finally { }
        }

        /// <summary>This method deletes a File in the specified path.</summary>
        /// <param name="FileName">Represents the complete path with the file name and extension. Example: "C:\Files\File.csv"</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        public void DeleteFile(string FileName, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;
                System.IO.File.Delete(FileName);
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
            }
            finally { }
        }
        #endregion

        #region [URL Methods]
        /// <summary>This method converts a Json string to System.Data.DataSet.</summary>
        /// <param name="Json">The Json string.</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        /// <returns>System.Data.DataSet</returns>
        public System.Data.DataSet ConvertJsonToDataSet(string Json, out string ExceptionMessage)
        {
            try
            {
                //JavaScriptSerializer oJs = new JavaScriptSerializer();

                ExceptionMessage = string.Empty; System.Data.DataSet ds;
                ds = JsonConvert.DeserializeObject<System.Data.DataSet>(Json);
                // ds = oJs.Deserialize<System.Data.DataSet>(Json);

                if (ds != null || ds != new System.Data.DataSet())
                {
                    return ds;
                }
                else throw new Exception("Dataset is null");
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                return new System.Data.DataSet();
            }
            finally { }
        }

        /// <summary>This method reads and provides the content as Syste.String from an URL.</summary>
        /// <param name="URL">The URL to query.</param>
        /// <param name="ExceptionMessage">Gets an exception message when the method fails.</param>
        /// <returns>System.String</returns>
        /* public String GetTextFromURL(string URL, out string ExceptionMessage)
        {
            try
            {
                ExceptionMessage = string.Empty;
                var oWC = new System.Net.WebClient();
                return oWC.DownloadString(URL);
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message; return string.Empty;
            }
            finally { }
        } */
        #endregion
        #region [mQuery Methods]
        /// <summary>
        /// Metodo mQuery :medodo que realiza consultas a la Base de Datos
        /// consultado store procedures sin enviar un parametro solo ejecutando,
        /// utilizando para ello una bandera variable booleana true.
        /// </summary>
        /// <param name="StoreProcedureName"></param>
        /// <param name="isProcedure"></param>
        /// <param name="pError"></param>
        /// <returns></returns>
        public DataTable mQuery(string StoreProcedureName, bool isProcedure, out string pError)
        {

            DataTable oDt = new DataTable();
            SqlConnection oCnx = new SqlConnection(vCn);
            SqlCommand oSqlCmd = new SqlCommand();
            oSqlCmd.CommandType = CommandType.StoredProcedure;
            oSqlCmd.CommandText = StoreProcedureName;
            oSqlCmd.Connection = oCnx;
            foreach (SqlParameter oParameter in lstParameters)
                oSqlCmd.Parameters.Add(oParameter);

            SqlDataAdapter oDa = new SqlDataAdapter(oSqlCmd);
            try
            {
                oCnx.Open();
                oDa.Fill(oDt);
                pError = "";
                return oDt;
            }
            catch (SqlException e)
            {
                pError = "[mConsultar]" + e.Message.ToString();
                return null;
            }
            finally
            {
                oCnx.Close();
            }
        }
        /// <summary>
        /// Metodo mQuery :medodo que realiza consultas a la Base de Datos
        /// consultado store procedures sin enviar un parametro solo ejecutando,
        /// utilizando para ello una bandera variable booleana true.
        /// </summary>
        /// <param name="StoreProcedureName"></param>
        /// <param name="isProcedure"></param>
        /// <param name="pError"></param>
        /// <returns></returns>
        public DataSet mQuery(string StoreProcedureName, bool isProcedure, bool returnDataset, out string pError)
        {
            DataSet oDs = new DataSet();
            SqlConnection oCnx = new SqlConnection(vCn);
            SqlCommand oSqlCmd = new SqlCommand();
            oSqlCmd.CommandType = CommandType.StoredProcedure;
            oSqlCmd.CommandText = StoreProcedureName;
            oSqlCmd.Connection = oCnx;
            foreach (SqlParameter oParameter in lstParameters)
                oSqlCmd.Parameters.Add(oParameter);

            SqlDataAdapter oDa = new SqlDataAdapter(oSqlCmd);
            try
            {
                oCnx.Open();
                oDa.Fill(oDs);
                pError = "";
                return oDs;
            }
            catch (SqlException e)
            {
                pError = "[mConsultar]" + e.Message.ToString();
                return null;
            }
            finally
            {
                oCnx.Close();
            }
        }
        /// <summary>
        /// Metodo para realizar consultas a la base de datos 
        /// a la que corresponde el objeto de conección
        /// </summary>
        /// <param name="Query">Parametro String con el query que quiere ejecutar</param>
        /// <param name="pError">Parametro que retorna un mensaje de error cuando no puede ejecutar el query</param>
        /// <returns>Retorna un DataTable con el resultado de la consulta</returns>
        public DataTable mQuery(string Query, out string pError)
        {
            DataTable oDt = new DataTable();
            SqlConnection oCnx = new SqlConnection(vCn);
            SqlDataAdapter oDa = new SqlDataAdapter(Query, oCnx);
            try
            {
                oCnx.Open();
                oDa.Fill(oDt);
                pError = "";
                return oDt;
            }
            catch (Exception e)
            {
                pError = "[mConsultar]" + e.Message.ToString();
                return null;
            }
            finally
            {
                oCnx.Close();
            }
        }
        /// <summary>
        /// Sobre carga del metodo consultar que permite ejecutar 
        /// Procedimientos almacenados recibiendo como parametro
        /// un arreglo de parametros (Tipo de dato propio de la clase).
        /// </summary>
        /// <param name="StoreProcedureName">String del procedimiento almacenado que desea ejecutar</param>
        /// /// <param name="pError">Parametro que retorna un mensaje de error cuando no puede ejecutar el query</param>
        /// <param name="Parametres">
        ///     Arreglo estructuras propio de la clase cCnx.
        ///     <example>
        ///     <para>----------------------------------------------------------------</para>
        ///     <para>--------------------------EJEMPLO---------------------------</para>
        ///     <para>----------------------------------------------------------------</para>
        ///     <para>int i=0;</para>
        ///     <para>DataTable oDt = new DataTable();</para>
        ///     <para>cCnx oCnx = new cCnx("172.24.76.202","Tivo", "usersql", "user");</para>
        ///     <para>oCnx.arrParametres[i].vName = "";</para>
        ///     <para>oCnx.arrParametres[i].vValue = "";</para>
        ///     <para>oCnx.arrParametres[i].vName = "@Prueba";</para>
        ///     <para>oCnx.arrParametres[i].vValue = "Functional";</para>
        ///     <para>oDt = oCnx.mConsultar("sp_returnall2", oCnx.arrParametres);</para>
        ///     <para>foreach (DataRow oRow in oDt.Rows)</para>
        ///     <para>{</para>
        ///     <para>  string vTexto = oRow["rutina"].ToString();</para>
        ///     <para>}</para>
        ///     </example>
        /// </param>
        /// 
        /// <returns>Retorna un DataTable con el resultado del procedimiento almacenado ejecutado</returns>
        public DataTable mQuery(string StoreProcedureName, estParametres[] Parametres, out string pError)
        {
            DataTable oDt = new DataTable();
            SqlConnection oCnx = new SqlConnection(vCn);
            SqlCommand oSqlCmd = new SqlCommand();
            oSqlCmd.CommandType = CommandType.StoredProcedure;
            oSqlCmd.CommandText = StoreProcedureName;
            oSqlCmd.Connection = oCnx;
            estParametres[] arrParametres = Parametres;
            for (int i = 0; i < arrParametres.Length; i++)
            {
                if (arrParametres[i].vValue != null)
                {
                    oSqlCmd.Parameters.AddWithValue(arrParametres[i].vName, arrParametres[i].vValue);
                }
            }
            SqlDataAdapter oDa = new SqlDataAdapter(oSqlCmd);
            try
            {
                oCnx.Open();
                oDa.Fill(oDt);
                pError = "";
                return oDt;
            }
            catch (SqlException e)
            {
                pError = "[mConsultar]" + e.Message.ToString();
                return null;
            }
            finally
            {
                oCnx.Close();
            }
        }
        /// <summary>
        /// Metodo Actualizar permite insertar nuevos registros en la base de datos, actualizarlos o borrarlos
        /// </summary>
        /// <param name="Query">
        /// Recibe un Query SQL que ejecutara en la base SQL 
        /// <para>Puede ser un Insert, Delete o Update</para>
        /// <example>
        /// <para>Ejemplo_1: insert into TablaPruebas(id,Nombre,aPaterno) values(1,'Alfredo','Rodriguez')</para>
        /// <para>Ejemplo_2: update TablaPruebas set aMaterno = 'Rodriguez' where id = 1</para>
        /// <para>Ejemplo_3: delete TablaPruebas where id = 1</para>
        /// </example>
        /// </param>
        /// <param name="ShowError">
        /// <para>Recibe un valor booleano que determina si la clase debe mostrar un error en un message box o lo omite</para>
        /// </param>
        /// <param name="pError">Parametro que retorna un mensaje de error cuando no puede ejecutar el query</param>
        /// <returns>Retorna un valor entero con la cantidad de filas afectadas</returns>
        public int mUpdate(string Query, out string pError)
        {
            SqlConnection oCnx = new SqlConnection(vCn);
            SqlCommand oSqlCmd = new SqlCommand(Query, oCnx);
            try
            {
                oCnx.Open();
                int vUpdated = oSqlCmd.ExecuteNonQuery();
                pError = "";
                return vUpdated;
            }
            catch (Exception e)
            {
                pError = "[mConsultar]" + e.Message.ToString();
                return -1;
            }
            finally
            {
                oCnx.Close();
            }
        }
        /// <summary>
        /// Metodo Consulta escalar retorna el campo de la primer columna y primer fila.
        /// </summary>
        /// <param name="Query">Recibe un Query de SQL que ejecutara en la base de datos
        ///     <example>
        ///         <para>Ejemplo_1: select count(*) from tabla pruebas</para>
        ///         <para>Ejemplo_2: select id from tabla pruebas</para>
        ///         <remarks>
        ///             <para>Cuando la consulta regresa mas de una columna y/o una fila, se toma la primera y las demas se omiten</para>
        ///             <para>El valor de retorno debe de ser un entero, con cadenas de caracteres no funcniona</para>
        ///         </remarks>
        ///     </example>
        /// </param>
        /// /// <param name="pError">Parametro que retorna un mensaje de error cuando no puede ejecutar el query</param>
        /// <returns>Retorna un valor entero con lo obtenido en la primer columna y primer fila del resultado del <paramref name="Query"/>.</returns>
        public int mScaleQuery(string Query, out string pError)
        {
            SqlConnection oCnx = new SqlConnection(vCn);
            SqlCommand oSqlCmd = new SqlCommand(Query, oCnx);
            try
            {
                oCnx.Open();
                int vScalar = (int)oSqlCmd.ExecuteScalar();
                pError = "";
                return vScalar;
            }
            catch (Exception e)
            {
                pError = "[mConsultar]" + e.Message.ToString();
                return 0;
            }
            finally
            {
                oCnx.Close();
            }
        }


        internal DataTable mQuery(string p)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}