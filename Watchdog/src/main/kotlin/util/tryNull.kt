package util

fun <T> tryNull(f: () -> T): T? = try {
    f()
} catch (_: Exception) {
    null
}

fun <T, R> T.tryNull(f: (T) -> R): R? =
    util.tryNull { f(this) }