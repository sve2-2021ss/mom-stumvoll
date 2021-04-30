package watchdog.notifications

import com.github.ajalt.mordant.rendering.TextColors.red

class ConsoleNotification : NotificationStrategy {
    override fun notify(device: String, messages: List<String>) {
        println(red(device))
        messages.forEach {
            println(red("\t$it"))
        }
    }
}