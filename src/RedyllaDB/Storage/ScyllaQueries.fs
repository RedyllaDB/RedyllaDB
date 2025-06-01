module internal RedyllaDB.Storage.ScyllaQueries

open System
open Cassandra

type KeyType =
    | String = 1s
    | HashSetItem = 2s
    | HashSetTTL = 3s

type ScyllaQueries = {    
    SetWithTTL: PreparedStatement
    Set: PreparedStatement
    GetString: PreparedStatement
    GetRecord: PreparedStatement
    ExpireKey: PreparedStatement
    GetExpireTimeString: PreparedStatement
    GetExpireTime: PreparedStatement
    DelKey: PreparedStatement
    DelKeyTTL: PreparedStatement
}

let init (session: ISession) =
    let initTables = """
        CREATE TABLE IF NOT EXISTS redis_data (
            key BLOB,
            key_type SMALLINT,
            hash_key BLOB,
            value BLOB,                                
            last_updated TIMESTAMP,                                                
            PRIMARY KEY (key, key_type, hash_key)
        );

        CREATE TABLE IF NOT EXISTS redis_ttl (
            partition_number INT,
            key BLOB,
            expire_at TIMESTAMP,                                                                 
            PRIMARY KEY (partition_number, key)
        );"""
    
    let statements = initTables.Split(';', StringSplitOptions.RemoveEmptyEntries)
    for st in statements do        
        session.Execute(st) |> ignore        
    
    let query = "INSERT INTO redis_data (key, key_type, hash_key, value, last_updated) VALUES (?, ?, ?, ?, toTimestamp(now())) USING TTL ?"
    let setWithTTL = session.Prepare query
    
    let query = "INSERT INTO redis_data (key, key_type, hash_key, value, last_updated) VALUES (?, ?, ?, ?, toTimestamp(now()))"
    let set = session.Prepare query
    
    let query = "SELECT value FROM redis_data WHERE key = ? AND key_type = 1"
    let getString = session.Prepare query
    
    let query = "SELECT key_type, hash_key, value, last_updated FROM redis_data WHERE key = ? LIMIT 1"
    let getRecord = session.Prepare query
    
    let query = "INSERT INTO redis_ttl (partition_number, key, expire_at) VALUES (?, ?, ?)"
    let expireKey = session.Prepare query
    
    let query = "SELECT key_type, TTL(value), last_updated FROM redis_data WHERE key = ?"
    let getExpireTimeString = session.Prepare query
    
    let query = "SELECT expire_at FROM redis_ttl WHERE partition_number = ? AND key = ?"
    let getExpireTime = session.Prepare query
    
    let query = "DELETE FROM redis_data WHERE key = ?"
    let delKey = session.Prepare query
    
    let query = "DELETE FROM redis_ttl WHERE partition_number = ? AND key = ?"
    let delKeyTTL = session.Prepare query
    
    { SetWithTTL = setWithTTL; Set = set; GetString = getString; GetRecord = getRecord
      ExpireKey = expireKey; GetExpireTimeString = getExpireTimeString; GetExpireTime = getExpireTime
      DelKey = delKey; DelKeyTTL = delKeyTTL }