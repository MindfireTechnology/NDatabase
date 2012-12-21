using System;
using NDatabase2.Odb;
using NDatabase2.Odb.Core.Query;
using NDatabase2.Odb.Core.Query.Criteria;
using NUnit.Framework;

namespace Test.NDatabase.Odb.Test.Query.Criteria
{
    [TestFixture]
    public class TestQueryOrderBy : ODBTest
    {
        [Test]
        public virtual void Test1()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            odb.Store(new Class1("c1"));
            odb.Store(new Class1("c1"));
            odb.Store(new Class1("c2"));
            odb.Store(new Class1("c2"));
            odb.Store(new Class1("c3"));
            odb.Store(new Class1("c4"));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<Class1>();
            q.OrderByAsc("name");
            var objects = q.Execute<Class1>();
            AssertEquals(6, objects.Count);
            while (objects.HasNext())
                Console.Out.WriteLine(objects.Next());
            // println(objects);
            odb.Close();
        }

        [Test]
        public virtual void Test2()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            odb.Store(new Class1("c1"));
            odb.Store(new Class1("c1"));
            odb.Store(new Class1("c2"));
            odb.Store(new Class1("c2"));
            odb.Store(new Class1("c3"));
            odb.Store(new Class1("c4"));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<Class1>();
            // q.orderByAsc("name");
            var objects = q.Execute<Class1>();
            AssertEquals(6, objects.Count);
            Println(objects);
            odb.Close();
        }

        [Test]
        public virtual void Test3()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            var size = 500;
            for (var i = 0; i < size; i++)
                odb.Store(new Class1("c1"));
            for (var i = 0; i < size; i++)
                odb.Store(new Class1("c2"));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<Class1>();
            // q.orderByAsc("name");
            var objects = q.Execute<Class1>();
            AssertEquals(size * 2, objects.Count);
            for (var i = 0; i < size; i++)
            {
                var c1 = objects.Next();
                AssertEquals("c1", c1.GetName());
            }
            for (var i = 0; i < size; i++)
            {
                var c1 = objects.Next();
                AssertEquals("c2", c1.GetName());
            }
            odb.Close();
        }

        [Test]
        public virtual void Test4()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            var size = 5;
            for (var i = 0; i < size; i++)
                odb.Store(new VO.Login.Function("f" + (i + 1)));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<VO.Login.Function>();
            // q.orderByAsc("name");
            var objects = q.Execute<VO.Login.Function>(true, 0, 2);
            AssertEquals(2, objects.Count);
            odb.Close();
            odb = Open(baseName);
            q = odb.Query<VO.Login.Function>();
            q.OrderByAsc("name");
            objects = q.Execute<VO.Login.Function>(true, 0, 2);
            AssertEquals(2, objects.Count);
            odb.Close();
            odb = Open(baseName);
            q = odb.Query<VO.Login.Function>();
            q.OrderByDesc("name");
            objects = q.Execute<VO.Login.Function>(true, 0, 2);
            AssertEquals(2, objects.Count);
            odb.Close();
        }

        [Test]
        public virtual void Test5()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            var size = 5;
            for (var i = 0; i < size; i++)
                odb.Store(new VO.Login.Function("f1"));
            odb.Store(new VO.Login.Function(null));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals().Not();
            // q.orderByAsc("name");
            var objects = q.Execute<VO.Login.Function>(true, 0, 10);
            AssertEquals(size, objects.Count);
            odb.Close();
            odb = Open(baseName);
            q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals().Not();
            q.OrderByAsc("name");
            objects = q.Execute<VO.Login.Function>(true, 0, 10);
            AssertEquals(5, objects.Count);
            odb.Close();
            odb = Open(baseName);
            q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals().Not();
            q.OrderByDesc("name");
            objects = q.Execute<VO.Login.Function>(true, 0, 10);
            AssertEquals(5, objects.Count);
            odb.Close();
        }

        [Test]
        public virtual void Test51()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            odb.Store(new VO.Login.Function("Not Null"));
            odb.Store(new VO.Login.Function(null));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals().Not();
            // q.orderByAsc("name");
            var objects = q.Execute<VO.Login.Function>(true, 0, 10);
            odb.Close();
            AssertEquals(1, objects.Count);
        }

        [Test]
        public virtual void Test6()
        {
            var baseName = GetBaseName();
            var odb = Open(baseName);
            var size = 5;
            for (var i = 0; i < size; i++)
                odb.Store(new VO.Login.Function("f1"));
            odb.Store(new VO.Login.Function(null));
            odb.Close();
            odb = Open(baseName);
            IQuery q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals();
            // q.orderByAsc("name");
            var objects = q.Execute<VO.Login.Function>(true, 0, 10);
            AssertEquals(1, objects.Count);
            odb.Close();
            odb = Open(baseName);
            q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals();
            q.OrderByAsc("name");
            objects = q.Execute<VO.Login.Function>(true, 0, 10);
            AssertEquals(1, objects.Count);
            odb.Close();
            odb = Open(baseName);
            q = odb.Query<VO.Login.Function>();
            q.Descend("name").Constrain(null).Equals();
            q.OrderByDesc("name");
            objects = q.Execute<VO.Login.Function>(true, 0, 10);
            AssertEquals(1, objects.Count);
            odb.Close();
        }
    }
}
