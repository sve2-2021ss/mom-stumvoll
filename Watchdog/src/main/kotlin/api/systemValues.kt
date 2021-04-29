package api

import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.Serializable
import kotlinx.serialization.protobuf.ProtoNumber

sealed class SystemValue

@ExperimentalSerializationApi
@Serializable
data class CpuLoad(@ProtoNumber(1) val loadPercentage: Int = 0) : SystemValue()

@ExperimentalSerializationApi
@Serializable
data class RamUsage(
    @ProtoNumber(1) val usedMb: Int = 0,
    @ProtoNumber(2) val totalMb: Int = 0
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
    @ProtoNumber(0)
    Start,

    @ProtoNumber(1)
    Stop
}