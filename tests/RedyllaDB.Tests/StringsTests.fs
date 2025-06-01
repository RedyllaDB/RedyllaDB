module StringsTests

open System
open System.Threading.Tasks
open StackExchange.Redis
open Xunit
open Swensen.Unquote

[<Fact>]
let ``String should support Get Set Delete`` () = task {
    use redis = ConnectionMultiplexer.Connect("localhost")
    use scylla = RedyllaDB.ConnectionMultiplexer.Connect("localhost", "demo")

    let rDb = redis.GetDatabase()
    let sDb = scylla.GetDatabase()
    
    let! _ = rDb.StringSetAsync("key", "value")
    let! _ = sDb.StringSetAsync("key", "value")

    let! rValue = rDb.StringGetAsync("key")
    let! sValue = sDb.StringGetAsync("key")

    let! rDel = rDb.KeyDeleteAsync("key")
    let! sDel = sDb.KeyDeleteAsync("key")

    // remove `key` second time
    let! rDel2 = rDb.KeyDeleteAsync("key")
    let! sDel2 = sDb.KeyDeleteAsync("key")

    // should get null
    let! rValue2 = rDb.StringGetAsync("key")
    let! sValue2 = sDb.StringGetAsync("key")

    test <@ sValue = rValue @>
    test <@ rDel = sDel @>
    test <@ rDel2 = false && sDel2 = true @>
    test <@ rValue2 = sValue2 @>      
}

[<Fact>]
let ``String should support TTL`` () = task {
    use redis = ConnectionMultiplexer.Connect("localhost")
    use scylla = RedyllaDB.ConnectionMultiplexer.Connect("localhost", "demo")
    let rDb = redis.GetDatabase()    
    let sDb = scylla.GetDatabase()    
    
    let! _ = rDb.StringSetAsync("key", "value")
    let! _ = sDb.StringSetAsync("key", "value")
    
    let! rExpireTime = rDb.KeyExpireTimeAsync("key")
    let! sExpireTime = sDb.KeyExpireTimeAsync("key")
    
    let! rTtlResult = rDb.KeyExpireAsync("key", TimeSpan.FromMinutes(1.0))
    let! sTtlResult = sDb.KeyExpireAsync("key", TimeSpan.FromMinutes(1.0))

    let! rExpireTime2 = rDb.KeyExpireTimeAsync("key")
    let! sExpireTime2 = sDb.KeyExpireTimeAsync("key")

    // update TTL
    let! rTtlResult2 = rDb.KeyExpireAsync("key", TimeSpan.FromSeconds(1.0))
    let! sTtlResult2 = sDb.KeyExpireAsync("key", TimeSpan.FromSeconds(1.0))

    do! Task.Delay 2_000

    let! rValue = rDb.StringGetAsync("key")
    let! sValue = sDb.StringGetAsync("key")
    
    let rExpireTime2 = rExpireTime2.Value.Minute
    let sExpireTime2 = sExpireTime2.Value.Minute
    
    test <@ rExpireTime = sExpireTime @>
    test <@ rTtlResult = sTtlResult @>
    test <@ rExpireTime2 = sExpireTime2 @>
    test <@ rTtlResult2 = sTtlResult2 @>
    test <@ sValue = RedisValue.Null @>
    test <@ sValue = rValue @>
}