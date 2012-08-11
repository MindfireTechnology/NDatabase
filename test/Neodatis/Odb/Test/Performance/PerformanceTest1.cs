using System;
using System.Threading;
using NDatabase.Odb;
using NDatabase.Odb.Core.Layers.Layer2.Instance;
using NDatabase.Odb.Core.Layers.Layer2.Meta;
using NDatabase.Odb.Impl.Core.Layers.Layer3.Buffer;
using NDatabase.Tool.Wrappers;
using NUnit.Framework;

namespace Test.Odb.Test.Performance
{
    public class PerformanceTest1 : ODBTest
    {
        public static int TestSize = 50000;

        public static readonly string OdbFileName = "perf.neodatis";

        [Test]
        public virtual void TestInsertSimpleObjectODB()
        {
            var reconnectStatus = OdbConfiguration.ReconnectObjectsToSession();
            //OdbConfiguration.setReconnectObjectsToSession(false);
            // Thread.sleep(20000);
            var doUpdate = true;
            var doDelete = true;
            // Configuration.setDatabaseCharacterEncoding(null);
            // LogUtil.logOn(FileSystemInterface.LOG_ID,true);
            // LogUtil.logOn(ObjectReader.LOG_ID,true);
            // Configuration.setUseLazyCache(true);
            var inMemory = true;
            // Configuration.monitorMemory(true);
            // Configuration.setUseModifiedClass(true);
            // Deletes the database file
            DeleteBase(OdbFileName);
            long t1 = 0;
            long t2 = 0;
            long t3 = 0;
            long t4 = 0;
            long t5 = 0;
            long t6 = 0;
            long t7 = 0;
            long t77 = 0;
            long t8 = 0;
            IOdb odb = null;
            IObjects<SimpleObject> l = null;
            SimpleObject so = null;
            // Insert TEST_SIZE objects
            Println("Inserting " + TestSize + " objects");
            t1 = OdbTime.GetCurrentTimeInMs();
            odb = Open(OdbFileName);
            var i = 0;
            // odb.getClassRepresentation(SimpleObject.class).addFullInstantiationHelper(new
            // SimpleObjectFullInstantiationHelper());
            for (i = 0; i < TestSize; i++)
            {
                object o = GetSimpleObjectInstance(i);
                odb.Store(o);
                if (i % 10000 == 0)
                {
                }
            }
            // println("i="+i);
            // Monitor.displayCurrentMemory(""+i,true);
            // println("Cache="+Dummy.getEngine(odb).getSession().getCache().toString());
            t2 = OdbTime.GetCurrentTimeInMs();
            // Closes the database
            odb.Close();
            // if(true)return;
            t3 = OdbTime.GetCurrentTimeInMs();
            Println("Retrieving " + TestSize + " objects");
            // Reopen the database
            odb = Open(OdbFileName);
            // Gets the TEST_SIZE objects
            l = odb.GetObjects<SimpleObject>(inMemory);
            t4 = OdbTime.GetCurrentTimeInMs();
            i = 0;
            while (l.HasNext())
            {
                object o = l.Next();
                if (i % 10000 == 0)
                {
                }
                // Monitor.displayCurrentMemory("select "+i,true);
                i++;
            }
            t5 = OdbTime.GetCurrentTimeInMs();
            if (doUpdate)
            {
                Println("Updating " + TestSize + " objects");
                i = 0;
                so = null;
                l.Reset();
                while (l.HasNext())
                {
                    so = l.Next();
                    so.SetName(so.GetName() + " updated");
                    odb.Store(so);
                    if (i % 10000 == 0)
                    {
                    }
                    // Monitor.displayCurrentMemory(""+i);
                    i++;
                }
            }
            t6 = OdbTime.GetCurrentTimeInMs();
            odb.Close();
            t7 = OdbTime.GetCurrentTimeInMs();
            if (doDelete)
            {
                Println("Deleting " + TestSize + " objects");
                odb = Open(OdbFileName);
                l = odb.GetObjects<SimpleObject>(inMemory);
                t77 = OdbTime.GetCurrentTimeInTicks();
                // println("After getting objects - before delete");
                i = 0;
                while (l.HasNext())
                {
                    so = l.Next();
                    if (!so.GetName().EndsWith("updated"))
                        throw new Exception("Update  not ok for " + so.GetName());
                    odb.Delete(so);
                    if (i % 10000 == 0)
                    {
                    }
                    // println("s="+i);
                    i++;
                }
                odb.Close();
            }
            Thread.Sleep(500);

            t8 = OdbTime.GetCurrentTimeInMs();
            OdbConfiguration.SetReconnectObjectsToSession(reconnectStatus);
            DisplayResult("ODB " + TestSize + " SimpleObject objects ", t1, t2, t3, t4, t5, t6, t7, t77, t8);
        }

        private SimpleObject GetSimpleObjectInstance(int i)
        {
            var so = new SimpleObject();
            so.SetDate(new DateTime());
            so.SetDuration(i);
            so.SetName("Bonjour, comment allez vous?" + i);
            return so;
        }

        private void DisplayResult(string @string, long t1, long t2, long t3, long t4, long t5, long t6, long t7,
                                   long t77, long t8)
        {
            var s1 = " total=" + (t8 - t1);
            var s2 = " total insert=" + (t3 - t1) + " -- " + "insert=" + (t2 - t1) + " commit=" + (t3 - t2) + " o/s=" +
                     TestSize / (float) ((t3 - t1)) * 1000;
            var s3 = " total select=" + (t5 - t3) + " -- " + "select=" + (t4 - t3) + " get=" + (t5 - t4) + " o/s=" +
                     TestSize / (float) ((t5 - t3)) * 1000;
            var s4 = " total update=" + (t7 - t5) + " -- " + "update=" + (t6 - t5) + " commit=" + (t7 - t6) + " o/s=" +
                     TestSize / (float) ((t7 - t5)) * 1000;
            var s5 = " total delete=" + (t8 - t7) + " -- " + "select=" + (t77 - t7) + " - delete=" + (t8 - t77) +
                     " o/s=" + TestSize / (float) ((t8 - t7)) * 1000;
            Println(@string + s1 + " | " + s2 + " | " + s3 + " | " + s4 + " | " + s5);
            var tinsert = t3 - t1;
            var tselect = t5 - t3;
            var tupdate = t7 - t5;
            var tdelete = t8 - t7;
            Println("Nb buffers ok = " + MultiBufferedIO.NbBufferOk + "   /   nb not ok = " +
                    MultiBufferedIO.NbBufferNotOk);
            Println("Nb flushs= " + MultiBufferedIO.NumberOfFlush + "   /   flush size = " +
                    MultiBufferedIO.TotalFlushSize + " / NbFlushForOverlap=" + MultiBufferedIO.NbFlushForOverlap);
            // println("Same position write = "+
            // MultiBufferedIO.nbSamePositionForWrite+
            // "   /   same pos for read = "+
            // MultiBufferedIO.nbSamePositionForRead);
            Println("Nb  =" + OdbType.Number);
            AssertTrue("Performance", tinsert < 1050);
            AssertTrue("Performance", tselect < 535);
            AssertTrue("Performance", tupdate < 582);
            AssertTrue("Performance", tdelete < 740);
        }

        /// <exception cref="System.Exception"></exception>
        public static void Main2(string[] args)
        {
            // Thread.sleep(15000);
            // OdbConfiguration.setMessageStreamerClass(HessianMessageStreamer.class);
            var pt = new PerformanceTest1();
            pt.TestInsertSimpleObjectODB();
        }
    }

    internal class SimpleObjectFullInstantiationHelper : IFullInstantiationHelper
    {
        #region IFullInstantiationHelper Members

        public virtual object Instantiate(NonNativeObjectInfo nnoi)
        {
            var so = new SimpleObject();
            return so;
        }

        #endregion
    }
}