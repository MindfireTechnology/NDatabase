﻿using System;
using System.Collections.Generic;
using NDatabase.Odb.Core.Query.Criteria;
using NDatabase.Odb.Impl.Core.Query.Criteria;
using NUnit.Framework;
using Test.Odb.Test.VO.Login;

namespace Test.Odb.Test.Update
{
    [TestFixture]
    public class TestSimpleUpdateObject : ODBTest
    {
        /// <exception cref="System.Exception"></exception>
        public override void TearDown()
        {
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void Test1()
        {
            DeleteBase("t1u.neodatis");
            var odb = Open("t1u.neodatis");
            var login = new VO.Login.Function("login");
            var logout = new VO.Login.Function("logout");
            odb.Store(login);
            Println("--------");
            odb.Store(login);
            odb.Store(logout);
            // odb.commit();
            odb.Close();
            odb = Open("t1u.neodatis");
            var l = odb.GetObjects<VO.Login.Function>(true);
            var f2 = l.GetFirst();
            f2.SetName("login function");
            odb.Store(f2);
            odb.Close();
            var odb2 = Open("t1u.neodatis");
            var f = odb2.GetObjects<VO.Login.Function>().GetFirst();
            AssertEquals("login function", f.GetName());
            odb2.Close();
            DeleteBase("t1u.neodatis");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void Test2()
        {
            DeleteBase("t2.neodatis");
            var odb = Open("t2.neodatis");
            var nbUsers = odb.GetObjects<User>().Count;
            var nbProfiles = odb.GetObjects<Profile>(true).Count;
            var nbFunctions = odb.GetObjects<VO.Login.Function>(true).Count;
            var login = new VO.Login.Function("login");
            var logout = new VO.Login.Function("logout");
            IList<VO.Login.Function> list = new List<VO.Login.Function>();
            list.Add(login);
            list.Add(logout);
            var profile = new Profile("operator", list);
            var olivier = new User("olivier smadja", "olivier@neodatis.com", profile);
            var aisa = new User("A√≠sa Galv√£o Smadja", "aisa@neodRMuatis.com", profile);
            odb.Store(olivier);
            odb.Store(aisa);
            odb.Commit();
            var users = odb.GetObjects<User>(true);
            var profiles = odb.GetObjects<Profile>(true);
            var functions = odb.GetObjects<VO.Login.Function>(true);
            odb.Close();
            // println("Users:"+users);
            Println("Profiles:" + profiles);
            Println("Functions:" + functions);
            odb = Open("t2.neodatis");
            var l = odb.GetObjects<User>(true);
            odb.Close();
            AssertEquals(nbUsers + 2, users.Count);
            var user2 = users.GetFirst();
            AssertEquals(olivier.ToString(), user2.ToString());
            AssertEquals(nbProfiles + 1, profiles.Count);
            AssertEquals(nbFunctions + 2, functions.Count);
            var odb2 = Open("t2.neodatis");
            var l2 = odb2.GetObjects<VO.Login.Function>(true);
            var function = l2.GetFirst();
            function.SetName("login function");
            odb2.Store(function);
            odb2.Close();
            var odb3 = Open("t2.neodatis");
            var l3 = odb3.GetObjects<User>(true);
            var i = 0;
            while (l3.HasNext() && i < Math.Min(2, l3.Count))
            {
                var user = l3.Next();
                AssertEquals("login function", string.Empty + user.GetProfile().GetFunctions()[0]);
                i++;
            }
            odb3.Close();
            DeleteBase("t2.neodatis");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void Test3()
        {
            DeleteBase("t1u2.neodatis");
            var odb = Open("t1u2.neodatis");
            var login = new VO.Login.Function(null);
            odb.Store(login);
            odb.Close();
            odb = Open("t1u2.neodatis");
            login = odb.GetObjects<VO.Login.Function>(new CriteriaQuery(Where.IsNull("name"))).GetFirst();
            AssertTrue(login.GetName() == null);
            login.SetName("login");
            odb.Store(login);
            odb.Close();
            odb = Open("t1u2.neodatis");
            login = odb.GetObjects<VO.Login.Function>().GetFirst();
            AssertTrue(login.GetName().Equals("login"));
            odb.Close();
            DeleteBase("t1u2.neodatis");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void Test5()
        {
            DeleteBase("t5.neodatis");
            var odb = Open("t5.neodatis");
            var nbFunctions = odb.Count(new CriteriaQuery(typeof (VO.Login.Function)));
            var nbProfiles = odb.Count(new CriteriaQuery(typeof (Profile)));
            var nbUsers = odb.Count(new CriteriaQuery(typeof (User)));
            var login = new VO.Login.Function("login");
            var logout = new VO.Login.Function("logout");
            var list = new List<VO.Login.Function>();
            list.Add(login);
            list.Add(logout);
            var profile = new Profile("operator", list);
            var olivier = new User("olivier smadja", "olivier@neodatis.com", profile);
            var aisa = new User("A√≠sa Galv√£o Smadja", "aisa@neodatis.com", profile);
            odb.Store(olivier);
            odb.Store(profile);
            odb.Commit();
            odb.Close();
            odb = Open("t5.neodatis");
            var users = odb.GetObjects<User>(true);
            var profiles = odb.GetObjects<Profile>(true);
            var functions = odb.GetObjects<VO.Login.Function>(true);
            odb.Close();
            AssertEquals(nbUsers + 1, users.Count);
            AssertEquals(nbProfiles + 1, profiles.Count);
            AssertEquals(nbFunctions + 2, functions.Count);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void Test6()
        {
            // LogUtil.objectWriterOn(true);
            DeleteBase("t6.neodatis");
            var odb = Open("t6.neodatis");
            var login = new VO.Login.Function("login");
            var logout = new VO.Login.Function("logout");
            var list = new List<VO.Login.Function>();
            list.Add(login);
            list.Add(logout);
            var profile = new Profile("operator", list);
            var olivier = new User("olivier smadja", "olivier@neodatis.com", profile);
            odb.Store(olivier);
            odb.Close();
            Println("----------");
            odb = Open("t6.neodatis");
            var users = odb.GetObjects<User>(true);
            var u1 = users.GetFirst();
            u1.GetProfile().SetName("operator 234567891011121314");
            odb.Store(u1);
            odb.Close();
            odb = Open("t6.neodatis");
            var profiles = odb.GetObjects<Profile>(true);
            AssertEquals(1, profiles.Count);
            var p1 = profiles.GetFirst();
            AssertEquals(u1.GetProfile().GetName(), p1.GetName());
        }
    }
}