# Multi-Layered Caching Redis Pub Sub

A simple multi-layered caching Redis pub sub implementation. Clients use memory cache and fall back to a cenetralized redis cache when the memory cache is not set. The clients also subscribe to redis to clear the memory cache should cache be cleared or set / updated.

# Environment Setup

1. Install [Redis for windows](https://github.com/tporadowski/redis/releases)
1. Install [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis)


# App Settings Setup

1. Assign the connection string to redis section

## API 

1. `~/cache` GET takes `key` as query param and returns the cached object if any.

2. `~/cache` DELETE takes `key` and `byPattern` as query params to delete a cache key or all keys matching the regex expression should `byPattern` be set to true.

3. `~/cache` POST takes `key` and `minutes` as query params and an object as body content to set the cache.

# Version History

1. 2023-12-13: Initial release v1.0.0 

# Disclaimer

This implementation was made for educational / training purposes only.

# License

License is [MIT](https://en.wikipedia.org/wiki/MIT_License)

# MISC

Birbia is coming
