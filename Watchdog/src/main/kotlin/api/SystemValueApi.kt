package api

import Config
import com.rabbitmq.client.Channel
import com.rabbitmq.client.DeliverCallback
import com.rabbitmq.client.ShutdownSignalException
import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.decodeFromByteArray
import kotlinx.serialization.protobuf.ProtoBuf
import util.eprintln

@ExperimentalSerializationApi
class SystemValueApi(private val config: Config, private val channel: Channel) {
    private fun decode(routingKey: String, data: ByteArray): SystemValue? {
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

    fun startConsume(onValue: (String, SystemValue) -> Unit) {
        val queue = channel.queueDeclare().queue
        channel.queueBind(queue, config.exchange, config.routingKey)

        val deliverCallback = DeliverCallback { _, delivery ->
            try {
                decode(delivery.envelope.routingKey, delivery.body)?.let {
                    onValue(delivery.envelope.routingKey.split(".").first(), it)
                }
            } catch (e: Exception) {
                e.message?.let { eprintln(it) }
            }
        }

        channel.basicConsume(queue, true, deliverCallback, this::shutdownCallback)
        readLine()
    }

    private fun shutdownCallback(consumerTag: String, sig: ShutdownSignalException) {
        eprintln("Shutdown: ${sig.message}")
    }
}