import kotlinx.serialization.Serializable

@Serializable
data class Config(
    val host: String,
    val exchange: String,
    val deviceMatch: String,
    val cpuMax: Int,
    val ramMax: Int,
    val delicateProcesses: List<String>
)
