﻿using NDatabase.Odb.Core.Layers.Layer2.Meta;
using NDatabase.Odb.Core.Oid;
using NDatabase.Odb.Core.Transaction;
using NDatabase.Odb.Impl.Core.Layers.Layer3.Block;
using NDatabase.Odb.Impl.Core.Layers.Layer3.Engine;
using NDatabase.Odb.Impl.Core.Layers.Layer3.Oid;
using NDatabase.Odb.Impl.Core.Oid;
using NDatabase.Odb.Impl.Core.Transaction;
using NDatabase.Odb.Impl.Tool;
using NDatabase.Tool;

namespace NDatabase.Odb.Core.Layers.Layer3.Engine
{
    public class FileSystemProcessor : IFileSystemProcessor
    {
        private const string LogId = "FileSystemProcessor";
        private const string LogIdDebug = "FileSystemProcessor.debug";

        public IFileSystemInterface FileSystemInterface { get; private set; }

        /// <summary>
        ///   Write the current transaction Id, out of transaction
        /// </summary>
        public void WriteLastTransactionId(ITransactionId transactionId)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderLastTransactionId, false);
            // FIXME This should always be written directly without transaction
            FileSystemInterface.WriteLong(transactionId.GetId1(), false, "last transaction id 1/2",
                           WriteAction.DirectWriteAction);

            FileSystemInterface.WriteLong(transactionId.GetId2(), false, "last transaction id 2/2",
                           WriteAction.DirectWriteAction);
        }

        public void BuildFileSystemInterface(IStorageEngine storageEngine, ISession session)
        {
            FileSystemInterface =  new FileSystemInterface("local-data", storageEngine.GetBaseIdentification(), true,
                                                OdbConfiguration.GetDefaultBufferSizeForData(), session);
        }

        /// <summary>
        ///   Write the status of the last odb close
        /// </summary>
        public void WriteLastOdbCloseStatus(bool ok, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderLastCloseStatusPosition, writeInTransaction);
            FileSystemInterface.WriteBoolean(ok, writeInTransaction, "odb last close status");
        }

        /// <summary>
        ///   Write the version in the database file
        /// </summary>
        public void WriteVersion(bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderVersionPosition, writeInTransaction);
            FileSystemInterface.WriteInt(StorageEngineConstant.CurrentFileFormatVersion, writeInTransaction,
                          "database file format version");
        }

        public IDatabaseId WriteDatabaseId(IStorageEngine storageEngine, long creationDate, bool writeInTransaction)
        {
            var databaseId = UUID.GetDatabaseId(creationDate);

            FileSystemInterface.WriteLong(databaseId.GetIds()[0], writeInTransaction, "database id 1/4",
                           WriteAction.DirectWriteAction);
            FileSystemInterface.WriteLong(databaseId.GetIds()[1], writeInTransaction, "database id 2/4",
                           WriteAction.DirectWriteAction);
            FileSystemInterface.WriteLong(databaseId.GetIds()[2], writeInTransaction, "database id 3/4",
                           WriteAction.DirectWriteAction);
            FileSystemInterface.WriteLong(databaseId.GetIds()[3], writeInTransaction, "database id 4/4",
                           WriteAction.DirectWriteAction);

            storageEngine.SetDatabaseId(databaseId);

            return databaseId;
        }

        /// <summary>
        ///   Write the number of classes in meta-model
        /// </summary>
        public void WriteNumberOfClasses(long number, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderNumberOfClassesPosition, writeInTransaction);
            FileSystemInterface.WriteLong(number, writeInTransaction, "nb classes", WriteAction.DirectWriteAction);
        }

        /// <summary>
        ///   Write the database characterEncoding
        /// </summary>
        public void WriteDatabaseCharacterEncoding(bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderDatabaseCharacterEncodingPosition,
                                  writeInTransaction);
            if (OdbConfiguration.HasEncoding())
                FileSystemInterface.WriteString(OdbConfiguration.GetDatabaseCharacterEncoding(), writeInTransaction, true, 50);
            else
                FileSystemInterface.WriteString(StorageEngineConstant.NoEncoding, writeInTransaction, false, 50);
        }

        // fsi.writeLong(oid.getObjectId(), writeInTransaction, label,
        // writeAction);
        public void WriteOid(OID oid, bool writeInTransaction, string label, int writeAction)
        {
            if (oid == null)
                FileSystemInterface.WriteLong(-1, writeInTransaction, label, writeAction);
            else
                FileSystemInterface.WriteLong(oid.ObjectId, writeInTransaction, label, writeAction);
        }

        /// <summary>
        ///   Resets the position of the first class of the metamodel.
        /// </summary>
        /// <remarks>
        ///   Resets the position of the first class of the metamodel. It Happens when database is being refactored
        /// </remarks>
        public void WriteFirstClassInfoOID(OID classInfoId, bool inTransaction)
        {
            long positionToWrite = StorageEngineConstant.DatabaseHeaderFirstClassOid;
            FileSystemInterface.SetWritePosition(positionToWrite, inTransaction);
            WriteOid(classInfoId, inTransaction, "first class info oid", WriteAction.DataWriteAction);
            if (OdbConfiguration.IsDebugEnabled(LogId))
                DLogger.Debug("Updating first class info oid at " + positionToWrite + " with oid " +
                              classInfoId);
        }

        /// <summary>
        ///   Writes the header of a block of type ID - a block that contains ids of objects and classes
        /// </summary>
        /// <param name="position"> Position at which the block must be written, if -1, take the next available position </param>
        /// <param name="idBlockSize"> The block size in byte </param>
        /// <param name="blockStatus"> The block status </param>
        /// <param name="blockNumber"> The number of the block </param>
        /// <param name="previousBlockPosition"> The position of the previous block of the same type </param>
        /// <param name="writeInTransaction"> To indicate if write must be done in transaction </param>
        /// <returns> The position of the id @ </returns>
        public long WriteIdBlock(long position, int idBlockSize, byte blockStatus, int blockNumber,
                                         long previousBlockPosition, bool writeInTransaction)
        {
            if (position == -1)
                position = FileSystemInterface.GetAvailablePosition();

            // LogUtil.fileSystemOn(true);
            // Updates the database header with the current id block position
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderCurrentIdBlockPosition, writeInTransaction);
            FileSystemInterface.WriteLong(position, false, "current id block position", WriteAction.DirectWriteAction);
            FileSystemInterface.SetWritePosition(position, writeInTransaction);
            FileSystemInterface.WriteInt(idBlockSize, writeInTransaction, "block size");

            // LogUtil.fileSystemOn(false);
            FileSystemInterface.WriteByte(BlockTypes.BlockTypeIds, writeInTransaction);
            FileSystemInterface.WriteByte(blockStatus, writeInTransaction);

            // prev position
            FileSystemInterface.WriteLong(previousBlockPosition, writeInTransaction, "prev block pos",
                           WriteAction.DirectWriteAction);

            // next position
            FileSystemInterface.WriteLong(-1, writeInTransaction, "next block pos", WriteAction.DirectWriteAction);
            FileSystemInterface.WriteInt(blockNumber, writeInTransaction, "id block number");
            FileSystemInterface.WriteLong(0, writeInTransaction, "id block max id", WriteAction.DirectWriteAction);
            FileSystemInterface.SetWritePosition(position + OdbConfiguration.GetIdBlockSize() - 1, writeInTransaction);
            FileSystemInterface.WriteByte(0, writeInTransaction);

            if (OdbConfiguration.IsDebugEnabled(LogIdDebug))
                DLogger.Debug(string.Format("After create block, available position is {0}", FileSystemInterface.GetAvailablePosition()));

            return position;
        }

        /// <summary>
        ///   Marks a block of type id as full, changes the status and the next block position
        /// </summary>
        /// <param name="blockPosition"> </param>
        /// <param name="nextBlockPosition"> </param>
        /// <param name="writeInTransaction"> </param>
        /// <returns> The block position @ </returns>
        public long MarkIdBlockAsFull(long blockPosition, long nextBlockPosition, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(blockPosition + StorageEngineConstant.BlockIdOffsetForBlockStatus, writeInTransaction);
            FileSystemInterface.WriteByte(BlockStatus.BlockFull, writeInTransaction);
            FileSystemInterface.SetWritePosition(blockPosition + StorageEngineConstant.BlockIdOffsetForNextBlock, writeInTransaction);

            FileSystemInterface.WriteLong(nextBlockPosition, writeInTransaction, "next id block pos",
                           WriteAction.DirectWriteAction);

            return blockPosition;
        }

        /// <summary>
        ///   Creates the header of the file
        /// </summary>
        /// <param name="storageEngine">Storage engine </param>
        /// <param name="creationDate"> The creation date </param>
        public void CreateEmptyDatabaseHeader(IStorageEngine storageEngine, long creationDate)
        {
            WriteVersion(false);
            var databaseId = WriteDatabaseId(storageEngine, creationDate, false);

            // Create the first Transaction Id
            ITransactionId tid = new TransactionIdImpl(databaseId, 0, 1);

            storageEngine.SetCurrentTransactionId(tid);

            WriteLastTransactionId(tid);
            WriteNumberOfClasses(0, false);
            WriteFirstClassInfoOID(StorageEngineConstant.NullObjectId, false);
            WriteLastOdbCloseStatus(false, false);
            WriteDatabaseCharacterEncoding(false);

            // This is the position of the first block id. But it will always
            // contain the position of the current id block
            FileSystemInterface.WriteLong(StorageEngineConstant.DatabaseHeaderFirstIdBlockPosition, false, "current id block position",
                           WriteAction.DirectWriteAction);

            // Write an empty id block
            WriteIdBlock(-1, OdbConfiguration.GetIdBlockSize(), BlockStatus.BlockNotFull, 1, -1, false);
            Flush();

            var currentBlockInfo = new CurrentIdBlockInfo
            {
                CurrentIdBlockPosition = StorageEngineConstant.DatabaseHeaderFirstIdBlockPosition,
                CurrentIdBlockNumber = 1,
                CurrentIdBlockMaxOid = OIDFactory.BuildObjectOID(0)
            };

            storageEngine.SetCurrentIdBlockInfos(currentBlockInfo);
        }

        /// <summary>
        ///   Associate an object OID to its position
        /// </summary>
        /// <param name="idType"> The type : can be object or class </param>
        /// <param name="idStatus"> The status of the OID </param>
        /// <param name="currentBlockIdPosition"> The current OID block position </param>
        /// <param name="oid"> The OID </param>
        /// <param name="objectPosition"> The position </param>
        /// <param name="writeInTransaction"> To indicate if write must be executed in transaction </param>
        /// <returns> @ </returns>
        public long AssociateIdToObject(byte idType, byte idStatus, long currentBlockIdPosition, OID oid,
                                                long objectPosition, bool writeInTransaction)
        {
            // Update the max id of the current block
            FileSystemInterface.SetWritePosition(currentBlockIdPosition + StorageEngineConstant.BlockIdOffsetForMaxId,
                                  writeInTransaction);

            FileSystemInterface.WriteLong(oid.ObjectId, writeInTransaction, "id block max id update",
                           WriteAction.PointerWriteAction);

            var l1 = (oid.ObjectId - 1) % OdbConfiguration.GetNbIdsPerBlock();

            var idPosition = currentBlockIdPosition + StorageEngineConstant.BlockIdOffsetForStartOfRepetition +
                             (l1) * OdbConfiguration.GetIdBlockRepetitionSize();

            // go to the next id position
            FileSystemInterface.SetWritePosition(idPosition, writeInTransaction);

            // id type
            FileSystemInterface.WriteByte(idType, writeInTransaction, "id type");

            // id
            FileSystemInterface.WriteLong(oid.ObjectId, writeInTransaction, "oid", WriteAction.PointerWriteAction);

            // id status
            FileSystemInterface.WriteByte(idStatus, writeInTransaction, "id status");

            // object position
            FileSystemInterface.WriteLong(objectPosition, writeInTransaction, "obj pos", WriteAction.PointerWriteAction);

            return idPosition;
        }

        /// <summary>
        ///   Updates the real object position of the object OID
        /// </summary>
        /// <param name="idPosition"> The OID position </param>
        /// <param name="objectPosition"> The real object position </param>
        /// <param name="writeInTransaction"> indicate if write must be done in transaction @ </param>
        public void UpdateObjectPositionForObjectOIDWithPosition(long idPosition, long objectPosition,
                                                                         bool writeInTransaction)
        {
           FileSystemInterface.SetWritePosition(idPosition, writeInTransaction);
           FileSystemInterface.WriteByte(IdTypes.Object, writeInTransaction, "id type");
           FileSystemInterface.SetWritePosition(idPosition + StorageEngineConstant.BlockIdRepetitionIdStatus, writeInTransaction);
           FileSystemInterface.WriteByte(IDStatus.Active, writeInTransaction);
           FileSystemInterface.WriteLong(objectPosition, writeInTransaction, "Updating object position of id",
                           WriteAction.PointerWriteAction);
        }

        /// <summary>
        ///   Udates the real class positon of the class OID
        /// </summary>
        public void UpdateClassPositionForClassOIDWithPosition(long idPosition, long objectPosition,
                                                                       bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(idPosition, writeInTransaction);
            FileSystemInterface.WriteByte(IdTypes.Class, writeInTransaction, "id type");
            FileSystemInterface.SetWritePosition(idPosition + StorageEngineConstant.BlockIdRepetitionIdStatus, writeInTransaction);
            FileSystemInterface.WriteByte(IDStatus.Active, writeInTransaction);
            FileSystemInterface.WriteLong(objectPosition, writeInTransaction, "Updating class position of id",
                           WriteAction.PointerWriteAction);
        }

        public void UpdateStatusForIdWithPosition(long idPosition, byte newStatus, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(idPosition + StorageEngineConstant.BlockIdRepetitionIdStatus, writeInTransaction);
            FileSystemInterface.WriteByte(newStatus, writeInTransaction, "Updating id status");
        }

        /// <summary>
        ///   Updates the instance related field of the class info into the database file Updates the number of objects, the first object oid and the next class oid
        /// </summary>
        /// <param name="classInfo"> The class info to be updated </param>
        /// <param name="writeInTransaction"> To specify if it must be part of a transaction @ </param>
        public void UpdateInstanceFieldsOfClassInfo(ClassInfo classInfo, bool writeInTransaction)
        {
            var currentPosition = FileSystemInterface.GetPosition();
            if (OdbConfiguration.IsDebugEnabled(LogIdDebug))
                DLogger.Debug("Start of updateInstanceFieldsOfClassInfo for " +
                              classInfo.GetFullClassName());
            var position = classInfo.GetPosition() + StorageEngineConstant.ClassOffsetClassNbObjects;
            FileSystemInterface.SetWritePosition(position, writeInTransaction);
            var nbObjects = classInfo.GetNumberOfObjects();
            FileSystemInterface.WriteLong(nbObjects, writeInTransaction, "class info update nb objects",
                           WriteAction.PointerWriteAction);
            WriteOid(classInfo.GetCommitedZoneInfo().First, writeInTransaction, "class info update first obj oid",
                     WriteAction.PointerWriteAction);
            WriteOid(classInfo.GetCommitedZoneInfo().Last, writeInTransaction, "class info update last obj oid",
                     WriteAction.PointerWriteAction);
            if (OdbConfiguration.IsDebugEnabled(LogIdDebug))
                DLogger.Debug("End of updateInstanceFieldsOfClassInfo for " +
                              classInfo.GetFullClassName());
            FileSystemInterface.SetWritePosition(currentPosition, writeInTransaction);
        }

        public void WriteBlockSizeAt(long writePosition, int blockSize, bool writeInTransaction, object @object)
        {
            if (blockSize < 0)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.NegativeBlockSize.AddParameter(writePosition).AddParameter(blockSize).AddParameter(
                        @object.ToString()));
            }
            var currentPosition = FileSystemInterface.GetPosition();
            FileSystemInterface.SetWritePosition(writePosition, writeInTransaction);
            FileSystemInterface.WriteInt(blockSize, writeInTransaction, "block size");
            // goes back where we were
            FileSystemInterface.SetWritePosition(currentPosition, writeInTransaction);
        }

        /// <summary>
        ///   Writes a class attribute info, an attribute of a class
        /// </summary>
        public void WriteClassAttributeInfo(IStorageEngine storageEngine, ClassAttributeInfo cai, bool writeInTransaction)
        {
            FileSystemInterface.WriteInt(cai.GetId(), writeInTransaction, "attribute id");
            FileSystemInterface.WriteBoolean(cai.IsNative(), writeInTransaction);
            if (cai.IsNative())
            {
                FileSystemInterface.WriteInt(cai.GetAttributeType().GetId(), writeInTransaction, "att odb type id");
                if (cai.GetAttributeType().IsArray())
                {
                    FileSystemInterface.WriteInt(cai.GetAttributeType().GetSubType().GetId(), writeInTransaction, "att array sub type");
                    // when the attribute is not native, then write its class info
                    // position
                    if (cai.GetAttributeType().GetSubType().IsNonNative())
                    {
                        FileSystemInterface.WriteLong(
                            storageEngine.GetSession(true).GetMetaModel().GetClassInfo(
                                cai.GetAttributeType().GetSubType().GetName(), true).GetId().ObjectId,
                            writeInTransaction, "class info id of array subtype", WriteAction.DataWriteAction);
                    }
                }
                // For enum, we write the class info id of the enum class
                if (cai.GetAttributeType().IsEnum())
                {
                    FileSystemInterface.WriteLong(
                        storageEngine.GetSession(true).GetMetaModel().GetClassInfo(cai.GetFullClassname(), true).GetId()
                            .ObjectId, writeInTransaction, "class info id", WriteAction.DataWriteAction);
                }
            }
            else
            {
                FileSystemInterface.WriteLong(
                    storageEngine.GetSession(true).GetMetaModel().GetClassInfo(cai.GetFullClassname(), true).GetId().
                        ObjectId, writeInTransaction, "class info id", WriteAction.DataWriteAction);
            }
            FileSystemInterface.WriteString(cai.GetName(), false, writeInTransaction);
            FileSystemInterface.WriteBoolean(cai.IsIndex(), writeInTransaction);
        }

        public void Close()
        {
            FileSystemInterface.Close();
            FileSystemInterface = null;
        }

        public void Flush()
        {
            FileSystemInterface.Flush();
        }

        /// <summary>
        ///   <pre>Class User{
        ///     private String name;
        ///     private Function function;
        ///     }
        ///     When an object of type User is stored, it stores a reference to its function object.
        /// </pre>
        /// </summary>
        /// <remarks>
        ///   <pre>Class User{
        ///     private String name;
        ///     private Function function;
        ///     }
        ///     When an object of type User is stored, it stores a reference to its function object.
        ///     If the function is set to another, the pointer to the function object must be changed.
        ///     for example, it was pointing to a function at the position 1407, the 1407 value is stored while
        ///     writing the USer object, let's say at the position 528. To make the user point to another function object (which exist at the position 1890)
        ///     The position 528 must be updated to 1890.</pre>
        /// </remarks>
        public void UpdateObjectReference(long positionWhereTheReferenceIsStored, OID newOid,
                                                  bool writeInTransaction)
        {
            var position = positionWhereTheReferenceIsStored;
            if (position < 0)
                throw new OdbRuntimeException(NDatabaseError.NegativePosition.AddParameter(position));
            FileSystemInterface.SetWritePosition(position, writeInTransaction);
            // Ids are always stored as negative value to differ from a position!
            var oid = StorageEngineConstant.NullObjectIdId;
            if (newOid != null)
                oid = -newOid.ObjectId;
            FileSystemInterface.WriteLong(oid, writeInTransaction, "object reference", WriteAction.PointerWriteAction);
        }
    }
}