package systemValues

import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.Serializable
import kotlinx.serialization.protobuf.ProtoNumber

@ExperimentalSerializationApi
@Serializable
data class ServiceEvent(
    @ProtoNumber(1) val executable: String,
    @ProtoNumber(2) val serviceEventType: ServiceEventType
) : SystemValue


@Serializable
enum class ServiceEventType {
    Start, Stop
}