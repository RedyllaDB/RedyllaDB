module internal RedyllaDB.Storage.ScyllaOperations

open System
open System.IO.Hashing
open System.Threading.Tasks
open Cassandra
open StackExchange.Redis

open RedyllaDB.Utils
open RedyllaDB.Storage.ScyllaQueries

let [<Literal>] EMPTY_HASH_KEY = ""
let [<Literal>] TTL_PARTITION_COUNT = 1

let calcPartitionNumber (key: byte[]) =
    let partitionCount = TTL_PARTITION_COUNT
    let hash = key |> XxHash32.HashToUInt32 |> int
    hash % partitionCount

let setString (queries: ScyllaQueries, session: ISession,
               key: RedisKey, value: RedisValue, expiry: TimeSpan voption, keepTtl: bool, flags: CommandFlags) = task {
    
    let k:byte[] = RedisKey.op_Implicit key
    let v:byte[] = RedisValue.op_Implicit value
    
    match expiry with
    | ValueSome exp when exp.TotalSeconds >= 1 ->        
        let ttl = int expiry.Value.TotalSeconds
        let query = queries.SetWithTTL.Bind(k, int16 KeyType.String, EMPTY_HASH_KEY, v, ttl)
        let! _ = session.ExecuteAsync query
        ()
        
    | ValueSome exp -> failwith "The expiry argument cannot be smaller than 1 second."        
        
    | ValueNone -> 
        let query = queries.Set.Bind(k, int16 KeyType.String, EMPTY_HASH_KEY, v)
        let! _ = session.ExecuteAsync query
        ()

    return true        
}

let getString (queries: ScyllaQueries, session: ISession, key: RedisKey, flags: CommandFlags) = task {    
    let k:byte[] = RedisKey.op_Implicit key
    let query = queries.GetString.Bind k
    let! rows = session.ExecuteAsync query
        
    let row = rows |> Seq.tryHeadV
    
    return
        match row with
        | ValueSome r -> RedisValue.op_Implicit(r.GetValue<byte[]>("value"))
        | ValueNone   -> RedisValue.Null        
}

let getStrings (queries: ScyllaQueries, session: ISession, keys: RedisKey[], flags: CommandFlags) = task {
    let! values = 
        keys
        |> Array.map(fun key -> getString(queries, session, key, flags))
        |> Task.WhenAll
        
    return values
}

let expireKey (queries: ScyllaQueries, session: ISession, key: RedisKey,
               expiry: TimeSpan voption, flags: CommandFlags) = task {
    
    match expiry with
    | ValueSome exp when exp.TotalSeconds >= 1 ->
        let k:byte[] = RedisKey.op_Implicit key
        let query = queries.GetRecord.Bind k
        let! rows = session.ExecuteAsync query
        
        match rows |> Seq.tryHeadV with
        | ValueSome row ->
            let keyType = row.GetValue<KeyType>("key_type")
            if keyType = KeyType.String then                
                let value = row.GetValue<byte[]>("value") |> RedisValue.op_Implicit
                let! _ = setString(queries, session, key, value, expiry, false, flags)
                return true
            else
                return false
                
        | ValueNone -> return false        
    
    | ValueSome exp ->
        failwith "The expiry argument cannot be smaller than 1 second."
        return false
    
    | ValueNone -> return false
}

let getExpireTime (queries: ScyllaQueries, session: ISession, key: RedisKey, flags: CommandFlags) = task {
    let k:byte[] = RedisKey.op_Implicit key    
    let stringKeyExpQuery = queries.GetExpireTimeString.Bind k
    let partitionNumber = calcPartitionNumber k
    let keyExpExpQuery = queries.GetExpireTime.Bind(partitionNumber, k)

    let keyTtlTask = session.ExecuteAsync keyExpExpQuery
    let strTtlTask = session.ExecuteAsync stringKeyExpQuery

    let! _ = Task.WhenAll(keyTtlTask, strTtlTask)
    
    let strRow = strTtlTask.Result |> Seq.tryHeadV
    
    return
        match strRow with
        | ValueSome row ->
            let keyType = row.GetValue<KeyType>("key_type")
            
            if keyType = KeyType.String then
                let ttl = row.GetValue<Nullable<int>>("ttl(value)") |> ValueOption.ofNullable
                let lastUpdated = row.GetValue<Nullable<DateTime>>("last_updated") |> ValueOption.ofNullable            
                
                match ttl, lastUpdated with
                | ValueSome ttl, ValueSome lastUpdated -> Nullable<_>(lastUpdated.AddSeconds ttl)
                | _ -> Nullable<_>()
            else
                let ttlRow = keyTtlTask.Result |> Seq.tryHeadV
                match ttlRow with
                | ValueSome ttl -> ttl.GetValue<Nullable<DateTime>>("expire_at")
                | ValueNone     -> Nullable<_>()
            
        | ValueNone -> Nullable<_>()
}

let delKey (queries: ScyllaQueries, session: ISession, key: RedisKey, flags: CommandFlags) = task {
    let k:byte[] = RedisKey.op_Implicit key    
    let query = queries.DelKey.Bind k
    let! _ = session.ExecuteAsync query
    
    let partitionNumber = calcPartitionNumber k
    let keyTtlQuery = queries.DelKeyTTL.Bind(partitionNumber, k)
    session.ExecuteAsync(keyTtlQuery) |> ignore
    
    return true
}