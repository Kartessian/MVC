using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{

    public class Account : IDisposable
    {
        private Database database_;

        public Account(Database database)
        {
            this.database_ = database;
        }

        public User GetUser(string email, string password)
        {

            return database_.GetRecords<User>(
                            new KeyValuePair<string, object>("@email", email),
                            new KeyValuePair<string, object>("@password", password)
                    ).FirstOrDefault();
        }

        public void SetLastVisit(int userId, string ip)
        {
            database_.ExecuteSQL("update users set lastVisit = NOW(), lastVisitIp = @ip where id = @id",
                    new KeyValuePair<string, object>("@ip", ip),
                    new KeyValuePair<string, object>("@id", userId)
                );
        }

        public void Dispose()
        {
            database_ = null;
        }
    }
}