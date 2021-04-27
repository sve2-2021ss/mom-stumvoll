import com.rabbitmq.client.ConnectionFactory
import com.rabbitmq.client.DeliverCallback
import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.decodeFromByteArray
import kotlinx.serialization.decodeFromString
import kotlinx.serialization.json.Json
import kotlinx.serialization.protobuf.ProtoBuf
import systemValues.CpuLoad
import systemValues.RamUsage
import systemValues.ServiceEvent
import systemValues.SystemValue
import util.eprintln
import util.tryNull


fun loadConfig(): Config? =
    Config::class.java.getResource("config.json")
        ?.readText()
        ?.tryNull { Json.decodeFromString<Config>(it) }

@ExperimentalSerializationApi
fun decode(routingKey: String, data: ByteArray): SystemValue? {
    return when {
        routingKey.contains(Regex(""".*\.metrics\.cpu\.load""")) -> {
            ProtoBuf.decodeFromByteArray<CpuLoad>(data)
        }
        routingKey.contains(Regex(""".*\.metrics\.ram\.usage""")) -> {
            ProtoBuf.decodeFromByteArray<RamUsage>(data)
        }
        routingKey.contains(Regex(""".*\.events.service.(started|stopped)""")) -> {
            ProtoBuf.decodeFromByteArray<ServiceEvent>(data)
        }
        else -> {
            null
        }
    }
}

@ExperimentalSerializationApi
fun main() {
    val config = loadConfig()

    if (config == null) {
        eprintln("could not load config")
        return
    }

    val factory = ConnectionFactory().apply {
        host = config.host
    }

    factory.newConnection().use { conn ->
        val channel = conn.createChannel()
        val queue = channel.queueDeclare().queue
        channel.queueBind(queue, config.exchange, "${config.deviceMatch}.#")

        val cb = DeliverCallback { _, delivery ->
            try {
                val se = decode(delivery.envelope.routingKey, delivery.body)
                println(" [x] Received  '${delivery.envelope.routingKey}':'$se'")
            } catch (e: Exception) {
                println(e)
            }
        }

        channel.basicConsume(queue, true, cb) { msg, _ -> eprintln("SHUTDOWN: $msg") }
        readLine()
    }
}