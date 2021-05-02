package api

import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.Serializable
import kotlinx.serialization.protobuf.ProtoNumber

@ExperimentalSerializationApi
@Serializable
sealed class SystemValue {
    abstract val deviceId: String
}

@ExperimentalSerializationApi
@Serializable
class Cpu(
    @ProtoNumber(1) val loadPercentage: Int = 0,
    @ProtoNumber(2) val powerDraw: Int = 0,
    @ProtoNumber(3) val coreTemps: List<Int>,
    @ProtoNumber(10) override val deviceId: String,
) : SystemValue()

@ExperimentalSerializationApi
@Serializable
data class Ram(
    @ProtoNumber(1) val usedMb: Int = 0,
    @ProtoNumber(2) val totalMb: Int = 0,
    @ProtoNumber(3) val memoryClock: Int = 0,
    @ProtoNumber(10) override val deviceId: String,
) : SystemValue()


@ExperimentalSerializationApi
sealed class Event : SystemValue()

@ExperimentalSerializationApi
@Serializable
data class ServiceEvent(
    @ProtoNumber(1) val executable: String = "",
    @ProtoNumber(2) val serviceEventType: ServiceEventType,
    @ProtoNumber(10) override val deviceId: String,
) : Event()


@Serializable
@ExperimentalSerializationApi
enum class ServiceEventType {
    @ProtoNumber(1)
    Start,

    @ProtoNumber(1)
    Stop
}