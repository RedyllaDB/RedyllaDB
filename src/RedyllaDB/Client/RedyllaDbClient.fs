module internal RedyllaDB.Client.RedyllaDbClient

open Cassandra
open RedyllaDB.Storage
open RedyllaDB.Storage.ScyllaQueries
open StackExchange.Redis

type RedyllaDbClient(session: ISession, queries: ScyllaQueries) =
    
    interface RedyllaDB.IDatabase with
        member this.StringGetAsync(key: RedisKey, flags: CommandFlags) =
            ScyllaOperations.getString(queries, session, key, flags)            
        
        member this.StringGetAsync(keys: RedisKey array, flags: CommandFlags) =
            ScyllaOperations.getStrings(queries, session, keys, flags)
            
        member this.StringSetAsync(key, value, expiry, keepTtl, flags) =
            let exp = expiry |> ValueOption.ofNullable
            ScyllaOperations.setString(queries, session, key, value, exp, keepTtl, flags)
            
        member this.KeyExpireAsync(key, expiry, flags) =
            let exp = expiry |> ValueOption.ofNullable
            ScyllaOperations.expireKey(queries, session, key, exp, flags)
            
        member this.KeyExpireTimeAsync(key, flags) =
            ScyllaOperations.getExpireTime(queries, session, key, flags)
            
        member this.KeyDeleteAsync(key, flags) =
            ScyllaOperations.delKey(queries, session, key, flags)