using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnitODB;
using UnitODB.SQLite;
using UnitODB.Linq;
using System.Configuration;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var container = CreateContainer())
            {
                container.Create<User>();
            }
            // Perform some operations with the context
        }

        [TestMethod]
        public void TestMethod2()
        {
            User user = new User { Name = "Peter Chan", Email = "peter@ppm.com.hk", Password = "kl;h3429fgj347g" };

            using (var container = CreateContainer())
            {
                container.RegisterAdd(user);

                container.Commit();
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            using (var container = CreateContainer())
            {
                var q = container.Query<User>(new string[] { "*" });

                int n = q.ToList<User>().Count;

                Assert.IsTrue(n > 1);
            } 
        }

        [TestMethod]
        public void TestMethod4()
        {
            using (var container = CreateContainer())
            {
                var q = container.Query<User>(new string[] { "*" }).Where("Name").Eq("Peter Chan");

                var list = q.ToList<User>();

                var guest = list[0];

                Assert.AreEqual("Peter Chan", guest.Name);
            }
        }

        [TestMethod]
        public void TestMethod5()
        {
            using (var container = CreateContainer())
            {
                var repo = new OdbRepository<User>(container);

                var qs = from user in repo
                         where user.Id == 2
                         select user;

                var list = qs.ToList();

                Assert.AreEqual(1, list.Count);
            }
        }

        [TestMethod]
        public void TestMethod6()
        {
            using (var container = CreateContainer())
            {
                var repo = new OdbRepository<User>(container);

                var n = repo.Count();

                Assert.IsTrue(n > 1); 
            }
        }

        [TestMethod]
        public void TestMethod7()
        {
            using (var container = CreateContainer())
            {
                var repo =  new OdbRepository<User>(container);
                
                var rs = repo.GetEntityQueryable<User>();

                var q = rs.Where(user => user.Id == 2).Select(user => user);

                var sql = q.ToString();

                var list = q.ToList();

                Assert.AreEqual(1, list.Count); 
            }
        }

        private static OdbContainer CreateContainer() {
            
            string file = "test.db";
            var connStr = string.Format("data source={0};version=3;pooling=True;page size=4096;cache size=8192;datetimekind=Local;datetimeformat=CurrentCulture;journal mode=Memory;legacy format=False;cache=shared", file);

            var provider = new SQLiteOdbProvider(connStr);

            return new OdbContainer(provider, 2);
        } 
    }
}
