package util

fun <T> tryNull(f: () -> T): T? {
    return try {
        f()
    } catch (_: Exception) {
        null
    }
}

fun <T, R> T.tryNull(f: (T) -> R): R? {
    return util.tryNull { f(this) }
}