namespace Spatial.Data.Importer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Sqlite bulk insert.
    /// Link: http://procbits.com/2009/09/08/sqlite-bulk-insert
    /// </summary>
    public class SqlBulkInsert
    {
        #region Constants

        /// <summary>The param delim.</summary>
        private const string ParamDelim = "@";

        #endregion

        #region Fields

        /// <summary>The begin insert text.</summary>
        private readonly string beginInsertText;

        /// <summary>The conn.</summary>
        private readonly SqlConnection conn;

        /// <summary>The parameters.</summary>
        private readonly IDictionary<string, SqlParameter> parameters = new Dictionary<string, SqlParameter>();

        /// <summary>The table name.</summary>
        private readonly string tableName;

        /// <summary>The cmd.</summary>
        private SqlCommand cmd;

        /// <summary>The counter.</summary>
        private uint counter;

        /// <summary>The txn.</summary>
        private SqlTransaction txn;

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="SqlBulkInsert"/> class.</summary>
        /// <param name="connection">The db connection.</param>
        /// <param name="tableName">The table name.</param>
        public SqlBulkInsert(SqlConnection connection, string tableName)
        {
            this.CommitMax = 10000;
            this.AllowBulkInsert = true;
            this.conn = connection;
            this.tableName = tableName;

            StringBuilder query = new StringBuilder(255);
            query.AppendFormat("INSERT INTO [{0}] (", tableName);
            this.beginInsertText = query.ToString();
        }

        #endregion

        #region Properties

        /// <summary>Gets or sets a value indicating whether allow bulk insert.</summary>
        public bool AllowBulkInsert { get; set; }

        /// <summary>Gets the command text.</summary>
        /// <exception cref="Exception">You must add at least one parameter.</exception>
        public string CommandText
        {
            get
            {
                if (this.parameters.Count < 1)
                {
                    throw new Exception("You must add at least one parameter.");
                }

                StringBuilder sb = new StringBuilder(255);
                sb.Append(this.beginInsertText);

                foreach (string param in this.parameters.Keys)
                {
                    sb.AppendFormat("[{0}], ", param);
                }

                sb.Remove(sb.Length - 2, 2);
                sb.Append(") VALUES (");

                foreach (string param in this.parameters.Keys)
                {
                    sb.AppendFormat("{0}{1}, ", ParamDelim, param);
                }

                sb.Remove(sb.Length - 2, 2);
                sb.Append(")");

                return sb.ToString();
            }
        }

        /// <summary>Gets or sets the commit max.</summary>
        [CLSCompliant(false)]
        public uint CommitMax { get; set; }

        /// <summary>Gets the param delimiter.</summary>
        public string ParamDelimiter
        {
            get
            {
                return ParamDelim;
            }
        }

        /// <summary>Gets the table name.</summary>
        public string TableName
        {
            get
            {
                return this.tableName;
            }
        }

        #endregion

        #region Methods

        /// <summary>Add parameter.</summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="type">Type of parameter.</param>
        /// <returns>Data Parameter <see cref="IDbDataParameter"/>.</returns>
        public SqlParameter AddParameter(string name, DbType type)
        {
            string parameterName = string.Format("{0}{1}", ParamDelim, name);
            SqlParameter param = new SqlParameter(parameterName, type);
            this.parameters.Add(name, param);
            return param;
        }

        /// <summary>Flush transaction.</summary>
        /// <exception cref="Exception">Could not commit transaction. See InnerException for more details.</exception>
        public void Flush()
        {
            try
            {
                if (this.txn != null)
                {
                    this.txn.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not commit transaction. See InnerException for more details", ex);
            }
            finally
            {
                if (this.txn != null)
                {
                    this.txn.Dispose();
                }

                this.txn = null;
                this.cmd = null;
                this.counter = 0;
            }
        }

        /// <summary>The insert.</summary>
        /// <param name="paramValues">The param values.</param>
        /// <exception cref="Exception">The values array count must be equal to the count of the number of parameters.</exception>
        public void Insert(object[] paramValues)
        {
            if (paramValues.Length != this.parameters.Count)
            {
                throw new Exception("The values array count must be equal to the count of the number of parameters.");
            }

            this.counter++;

            if (this.counter == 1)
            {
                if (this.AllowBulkInsert)
                {
                    this.txn = this.conn.BeginTransaction();
                }

                this.cmd = this.conn.CreateCommand();
                foreach (SqlParameter par in this.parameters.Values)
                {
                    this.cmd.Parameters.Add(par);
                }

                this.cmd.CommandText = this.CommandText;
            }

            int i = 0;
            foreach (SqlParameter par in this.parameters.Values)
            {
                par.Value = paramValues[i];
                i++;
            }

            if (this.AllowBulkInsert)
            {
                this.cmd.Transaction = this.txn;
            }
            
            this.cmd.ExecuteNonQuery();

            if (this.counter == this.CommitMax)
            {
                try
                {
                    if (this.txn != null)
                    {
                        this.txn.Commit();
                    }
                }
                catch (Exception)
                {
                    Debug.Print("Exception");
                }
                finally
                {
                    if (this.txn != null)
                    {
                        this.txn.Dispose();
                        this.txn = null;
                    }

                    this.counter = 0;
                }
            }
        }

        #endregion
    }
}