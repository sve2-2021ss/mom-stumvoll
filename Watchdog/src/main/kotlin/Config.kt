import kotlinx.serialization.Serializable
import kotlinx.serialization.decodeFromString
import kotlinx.serialization.json.Json
import util.tryNull

@Serializable
data class Config(
    val host: String,
    val exchange: String,
    val routingKey: String,
    val cpuMax: Int,
    val ramMax: Int,
    val delicateProcesses: List<String>
) {
    companion object {
        fun fromFile(fileName: String): Config? =
            Config::class.java.getResource(fileName)
                ?.readText()
                ?.tryNull { Json.decodeFromString<Config>(it) }
    }
}
