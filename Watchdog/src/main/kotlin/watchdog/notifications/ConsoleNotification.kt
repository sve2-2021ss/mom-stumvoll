package watchdog.notifications

import com.github.ajalt.mordant.rendering.TextColors.red

class ConsoleNotification : NotificationStrategy {
    override fun notify(device: String, message: String) {
        println(red("$device --> $message"))
    }
}