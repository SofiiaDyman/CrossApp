namespace Core;

public sealed class StorageException(string message, Exception inner) : Exception(message, inner);
