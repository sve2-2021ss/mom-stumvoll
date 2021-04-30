package watchdog.notifications

interface NotificationStrategy {
    fun notify(device: String, messages: List<String>)
}