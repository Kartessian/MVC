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

        public bool EmailExist(string email)
        {
            return (long)database_.ExecuteScalar("select count(*) from `users` where `email` = @email",
                    new KeyValuePair<string, object>("@email",email)
                ) > 0;
        }

        public void CreateUser(string email, string name, string password, string ip)
        {
            string sSQL = "insert into `users` (`name`,`email`,`password`,`createdon`,`createdIP`) values (@name,@email,@password,NOW(),'" + ip + "')";
            database_.ExecuteSQL(sSQL, new KeyValuePair<string, object>[] {
                                                                                new KeyValuePair<string,object>("@name",name),
                                                                                new KeyValuePair<string,object>("@email",email),
                                                                                new KeyValuePair<string,object>("@password",password),
                                                                            });
        }

        public void Dispose()
        {
            database_ = null;
        }
    }
}