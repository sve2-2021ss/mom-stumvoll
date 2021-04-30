package api

import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.Serializable
import kotlinx.serialization.protobuf.ProtoNumber

sealed class SystemValue

@ExperimentalSerializationApi
@Serializable
data class Cpu(
    @ProtoNumber(1) val loadPercentage: Int = 0,
    @ProtoNumber(2) val powerDraw: Int = 0,
    @ProtoNumber(3) val coreTemps: List<Int>
) : SystemValue()

@ExperimentalSerializationApi
@Serializable
data class Ram(
    @ProtoNumber(1) val usedMb: Int = 0,
    @ProtoNumber(2) val totalMb: Int = 0,
    @ProtoNumber(3) val memoryClock: Int = 0
) : SystemValue()


@ExperimentalSerializationApi
@Serializable
data class ServiceEvent(
    @ProtoNumber(1) val executable: String = "",
    @ProtoNumber(2) val serviceEventType: ServiceEventType
) : SystemValue()


@Serializable
@ExperimentalSerializationApi
enum class ServiceEventType {
    @ProtoNumber(1)
    Start,

    @ProtoNumber(2)
    Stop
}