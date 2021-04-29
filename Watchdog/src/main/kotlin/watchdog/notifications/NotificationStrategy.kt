package watchdog.notifications

interface NotificationStrategy {
    fun notify(device: String, message: String)
}