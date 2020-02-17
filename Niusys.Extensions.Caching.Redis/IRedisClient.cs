using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Niusys.Extensions.Caching.Redis
{
    public interface IRedisClient
    {        //
        // 摘要:
        //     Decrements the number stored at key by decrement. If the key does not exist,
        //     it is set to 0 before performing the operation. An error is returned if the key
        //     contains a value of the wrong type or contains a string that is not representable
        //     as integer. This operation is limited to 64 bit signed integers.
        //
        // 返回结果:
        //     the value of key after the increment
        //
        // 备注:
        //     http://redis.io/commands/decrby
        Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Decrements the string representing a floating point number stored at key by the
        //     specified increment. If the key does not exist, it is set to 0 before performing
        //     the operation. The precision of the output is fixed at 17 digits after the decimal
        //     point regardless of the actual internal precision of the computation.
        //
        // 返回结果:
        //     the value of key after the increment
        //
        // 备注:
        //     http://redis.io/commands/incrbyfloat
        Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Returns the specified elements of the list stored at key. The offsets start and
        //     stop are zero-based indexes, with 0 being the first element of the list (the
        //     head of the list), 1 being the next element and so on. These offsets can also
        //     be negative numbers indicating offsets starting at the end of the list.For example,
        //     -1 is the last element of the list, -2 the penultimate, and so on. Note that
        //     if you have a list of numbers from 0 to 100, LRANGE list 0 10 will return 11
        //     elements, that is, the rightmost item is included.
        //
        // 返回结果:
        //     list of elements in the specified range.
        //
        // 备注:
        //     http://redis.io/commands/lrange
        RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Insert the specified value at the tail of the list stored at key. If key does
        //     not exist, it is created as empty list before performing the push operation.
        //
        // 返回结果:
        //     the length of the list after the push operation.
        //
        // 备注:
        //     http://redis.io/commands/rpush
        long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Insert all the specified values at the tail of the list stored at key. If key
        //     does not exist, it is created as empty list before performing the push operation.
        //     Elements are inserted one after the other to the tail of the list, from the leftmost
        //     element to the rightmost element. So for instance the command RPUSH mylist a
        //     b c will result into a list containing a as first element, b as second element
        //     and c as third element.
        //
        // 返回结果:
        //     the length of the list after the push operation.
        //
        // 备注:
        //     http://redis.io/commands/rpush
        long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns the value associated with field in the hash stored at key.
        //
        // 返回结果:
        //     the value associated with field, or nil when field is not present in the hash
        //     or key does not exist.
        //
        // 备注:
        //     http://redis.io/commands/hget
        RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns the value associated with field in the hash stored at key.
        //
        // 返回结果:
        //     the value associated with field, or nil when field is not present in the hash
        //     or key does not exist.
        //
        // 备注:
        //     http://redis.io/commands/hget
        Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Sets field in the hash stored at key to value. If key does not exist, a new key
        //     holding a hash is created. If field already exists in the hash, it is overwritten.
        //
        // 返回结果:
        //     1 if field is a new field in the hash and value was set. 0 if field already exists
        //     in the hash and the value was updated.
        //
        // 备注:
        //     http://redis.io/commands/hset
        bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Sets field in the hash stored at key to value. If key does not exist, a new key
        //     holding a hash is created. If field already exists in the hash, it is overwritten.
        //
        // 返回结果:
        //     1 if field is a new field in the hash and value was set. 0 if field already exists
        //     in the hash and the value was updated.
        //
        // 备注:
        //     http://redis.io/commands/hset
        Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Sets the specified fields to their respective values in the hash stored at key.
        //     This command overwrites any existing fields in the hash. If key does not exist,
        //     a new key holding a hash is created.
        //
        // 备注:
        //     http://redis.io/commands/hmset
        Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns if field is an existing field in the hash stored at key.
        //
        // 返回结果:
        //     1 if the hash contains field. 0 if the hash does not contain field, or key does
        //     not exist.
        //
        // 备注:
        //     http://redis.io/commands/hexists
        bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns if field is an existing field in the hash stored at key.
        //
        // 返回结果:
        //     1 if the hash contains field. 0 if the hash does not contain field, or key does
        //     not exist.
        //
        // 备注:
        //     http://redis.io/commands/hexists
        Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Removes the specified fields from the hash stored at key. Non-existing fields
        //     are ignored. Non-existing keys are treated as empty hashes and this command returns
        //     0.
        //
        // 返回结果:
        //     The number of fields that were removed.
        //
        // 备注:
        //     http://redis.io/commands/hdel
        Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Set a timeout on key. After the timeout has expired, the key will automatically
        //     be deleted. A key with an associated timeout is said to be volatile in Redis
        //     terminology.
        //
        // 返回结果:
        //     1 if the timeout was set. 0 if key does not exist or the timeout could not be
        //     set.
        //
        // 备注:
        //     If key is updated before the timeout has expired, then the timeout is removed
        //     as if the PERSIST command was invoked on key. For Redis versions < 2.1.3, existing
        //     timeouts cannot be overwritten. So, if key already has an associated timeout,
        //     it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout
        //     of a key. It is also possible to remove the timeout using the PERSIST command.
        //     See the page on key expiry for more information.
        bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Set a timeout on key. After the timeout has expired, the key will automatically
        //     be deleted. A key with an associated timeout is said to be volatile in Redis
        //     terminology.
        //
        // 返回结果:
        //     1 if the timeout was set. 0 if key does not exist or the timeout could not be
        //     set.
        //
        // 备注:
        //     If key is updated before the timeout has expired, then the timeout is removed
        //     as if the PERSIST command was invoked on key. For Redis versions < 2.1.3, existing
        //     timeouts cannot be overwritten. So, if key already has an associated timeout,
        //     it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout
        //     of a key. It is also possible to remove the timeout using the PERSIST command.
        //     See the page on key expiry for more information.
        Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Set a timeout on key. After the timeout has expired, the key will automatically
        //     be deleted. A key with an associated timeout is said to be volatile in Redis
        //     terminology.
        //
        // 返回结果:
        //     1 if the timeout was set. 0 if key does not exist or the timeout could not be
        //     set.
        //
        // 备注:
        //     If key is updated before the timeout has expired, then the timeout is removed
        //     as if the PERSIST command was invoked on key. For Redis versions < 2.1.3, existing
        //     timeouts cannot be overwritten. So, if key already has an associated timeout,
        //     it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout
        //     of a key. It is also possible to remove the timeout using the PERSIST command.
        //     See the page on key expiry for more information.
        Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None);


        //
        // 摘要:
        //     Removes the specified key. A key is ignored if it does not exist.
        //
        // 返回结果:
        //     True if the key was removed.
        //
        // 备注:
        //     http://redis.io/commands/del
        Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Set key to hold the string value. If key already holds a value, it is overwritten,
        //     regardless of its type.
        //
        // 备注:
        //     http://redis.io/commands/set
        Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Get the value of key. If the key does not exist the special value nil is returned.
        //     An error is returned if the value stored at key is not a string, because GET
        //     only handles string values.
        //
        // 返回结果:
        //     the value of key, or nil when key does not exist.
        //
        // 备注:
        //     http://redis.io/commands/get
        Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns all fields and values of the hash stored at key.
        //
        // 返回结果:
        //     list of fields and their values stored in the hash, or an empty list when key
        //     does not exist.
        //
        // 备注:
        //     http://redis.io/commands/hgetall
        Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Increments the number stored at key by increment. If the key does not exist,
        //     it is set to 0 before performing the operation. An error is returned if the key
        //     contains a value of the wrong type or contains a string that is not representable
        //     as integer. This operation is limited to 64 bit signed integers.
        //
        // 返回结果:
        //     the value of key after the increment
        //
        // 备注:
        //     http://redis.io/commands/incrby
        Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Increment the string representing a floating point number stored at key by the
        //     specified increment. If the key does not exist, it is set to 0 before performing
        //     the operation. The precision of the output is fixed at 17 digits after the decimal
        //     point regardless of the actual internal precision of the computation.
        //
        // 返回结果:
        //     the value of key after the increment
        //
        // 备注:
        //     http://redis.io/commands/incrbyfloat
        Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns the specified range of elements in the sorted set stored at key. By default
        //     the elements are considered to be ordered from the lowest to the highest score.
        //     Lexicographical order is used for elements with equal score. Both start and stop
        //     are zero-based indexes, where 0 is the first element, 1 is the next element and
        //     so on. They can also be negative numbers indicating offsets from the end of the
        //     sorted set, with -1 being the last element of the sorted set, -2 the penultimate
        //     element and so on.
        //
        // 返回结果:
        //     list of elements in the specified range
        //
        // 备注:
        //     http://redis.io/commands/zrange
        Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns if key exists.
        //
        // 返回结果:
        //     1 if the key exists. 0 if the key does not exist.
        //
        // 备注:
        //     http://redis.io/commands/exists
        Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Adds the specified member with the specified score to the sorted set stored at
        //     key. If the specified member is already a member of the sorted set, the score
        //     is updated and the element reinserted at the right position to ensure the correct
        //     ordering.
        //
        // 返回结果:
        //     True if the value was added, False if it already existed (the score is still
        //     updated)
        //
        // 备注:
        //     http://redis.io/commands/zadd
        Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags);
        //
        // 摘要:
        //     Adds the specified member with the specified score to the sorted set stored at
        //     key. If the specified member is already a member of the sorted set, the score
        //     is updated and the element reinserted at the right position to ensure the correct
        //     ordering.
        //
        // 返回结果:
        //     True if the value was added, False if it already existed (the score is still
        //     updated)
        //
        // 备注:
        //     http://redis.io/commands/zadd
        Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, When when = When.Always, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Adds all the specified members with the specified scores to the sorted set stored
        //     at key. If a specified member is already a member of the sorted set, the score
        //     is updated and the element reinserted at the right position to ensure the correct
        //     ordering.
        //
        // 返回结果:
        //     The number of elements added to the sorted sets, not including elements already
        //     existing for which the score was updated.
        //
        // 备注:
        //     http://redis.io/commands/zadd
        Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags);
        //
        // 摘要:
        //     Adds all the specified members with the specified scores to the sorted set stored
        //     at key. If a specified member is already a member of the sorted set, the score
        //     is updated and the element reinserted at the right position to ensure the correct
        //     ordering.
        //
        // 返回结果:
        //     The number of elements added to the sorted sets, not including elements already
        //     existing for which the score was updated.
        //
        // 备注:
        //     http://redis.io/commands/zadd
        Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, When when = When.Always, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns the specified range of elements in the sorted set stored at key. By default
        //     the elements are considered to be ordered from the lowest to the highest score.
        //     Lexicographical order is used for elements with equal score. Start and stop are
        //     used to specify the min and max range for score values. Similar to other range
        //     methods the values are inclusive.
        //
        // 返回结果:
        //     list of elements in the specified score range
        //
        // 备注:
        //     http://redis.io/commands/zrangebyscore
        Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns the sorted set cardinality (number of elements) of the sorted set stored
        //     at key.
        //
        // 返回结果:
        //     the cardinality (number of elements) of the sorted set, or 0 if key does not
        //     exist.
        //
        // 备注:
        //     http://redis.io/commands/zcard
        Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Removes all elements in the sorted set stored at key with rank between start
        //     and stop. Both start and stop are 0 -based indexes with 0 being the element with
        //     the lowest score. These indexes can be negative numbers, where they indicate
        //     offsets starting at the element with the highest score. For example: -1 is the
        //     element with the highest score, -2 the element with the second highest score
        //     and so forth.
        //
        // 返回结果:
        //     the number of elements removed.
        //
        // 备注:
        //     http://redis.io/commands/zremrangebyrank
        Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Returns if member is a member of the set stored at key.
        //
        // 返回结果:
        //     1 if the element is a member of the set. 0 if the element is not a member of
        //     the set, or if key does not exist.
        //
        // 备注:
        //     http://redis.io/commands/sismember
        Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Add the specified member to the set stored at key. Specified members that are
        //     already a member of this set are ignored. If key does not exist, a new set is
        //     created before adding the specified members.
        //
        // 返回结果:
        //     True if the specified member was not already present in the set, else False
        //
        // 备注:
        //     http://redis.io/commands/sadd
        Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Add the specified members to the set stored at key. Specified members that are
        //     already a member of this set are ignored. If key does not exist, a new set is
        //     created before adding the specified members.
        //
        // 返回结果:
        //     the number of elements that were added to the set, not including all the elements
        //     already present into the set.
        //
        // 备注:
        //     http://redis.io/commands/sadd
        Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        //
        // 摘要:
        //     Returns the set cardinality (number of elements) of the set stored at key.
        //
        // 返回结果:
        //     the cardinality (number of elements) of the set, or 0 if key does not exist.
        //
        // 备注:
        //     http://redis.io/commands/scard
        Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None);

        Task<RedisResult> ExecuteAsync(string command, params object[] args);

        Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Execute a lua script against the server, using previously prepared script. Named
        //     parameters, if any, are provided by the `parameters` object.
        //
        // 参数:
        //   script:
        //     The script to execute.
        //
        //   parameters:
        //     The parameters to pass to the script.
        //
        //   flags:
        //     The flags to use for this operation.
        //
        // 返回结果:
        //     A dynamic representation of the script's result
        //
        // 备注:
        //     https://redis.io/commands/eval
        Task<RedisResult> ScriptEvaluateAsync(LuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None);
        //
        // 摘要:
        //     Execute a lua script against the server, using previously prepared and loaded
        //     script. This method sends only the SHA1 hash of the lua script to Redis. Named
        //     parameters, if any, are provided by the `parameters` object.
        //
        // 参数:
        //   script:
        //     The already-loaded script to execute.
        //
        //   parameters:
        //     The parameters to pass to the script.
        //
        //   flags:
        //     The flags to use for this operation.
        //
        // 返回结果:
        //     A dynamic representation of the script's result
        //
        // 备注:
        //     https://redis.io/commands/eval
        Task<RedisResult> ScriptEvaluateAsync(LoadedLuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None);

        Task SafeClearRedisKeyPattern(string keyPattern);

        Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None);

        Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);
    }
}
