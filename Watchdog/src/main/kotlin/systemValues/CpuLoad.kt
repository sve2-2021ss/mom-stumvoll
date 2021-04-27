package systemValues

import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.Serializable
import kotlinx.serialization.protobuf.ProtoNumber

@ExperimentalSerializationApi
@Serializable
data class CpuLoad(@ProtoNumber(1) val loadPercentage: Int = 0) : SystemValue
