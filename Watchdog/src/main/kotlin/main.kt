import api.SystemValueApi
import com.rabbitmq.client.ConnectionFactory
import kotlinx.serialization.ExperimentalSerializationApi
import util.eprintln
import watchdog.Watchdog
import watchdog.notifications.ConsoleNotification

@ExperimentalSerializationApi
fun main() {
    val config = Config.fromFile("config.json")

    if (config == null) {
        eprintln("could not load config")
        return
    }

    val factory = ConnectionFactory().apply {
        host = config.host
    }

    val watchdog = Watchdog(config, ConsoleNotification())

    factory.newConnection().use {
        val channel = it.createChannel()
        val systemValueApi = SystemValueApi(config, channel)

        systemValueApi.startConsume(watchdog::onNewValue)
        readLine()
    }
}