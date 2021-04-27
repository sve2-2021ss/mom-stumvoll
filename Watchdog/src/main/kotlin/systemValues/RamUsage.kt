package systemValues

import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.Serializable
import kotlinx.serialization.protobuf.ProtoNumber

@ExperimentalSerializationApi
@Serializable
data class RamUsage(
    @ProtoNumber(1) val usedMb: Int = 0,
    @ProtoNumber(2) val totalMb: Int = 0
) : SystemValue
