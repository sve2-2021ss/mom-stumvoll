import kotlinx.serialization.Serializable
import kotlinx.serialization.decodeFromString
import kotlinx.serialization.json.Json
import util.tryNull

@Serializable
data class Config(
    val host: String,
    val exchange: String,
    val routingKey: String,
    val cpuConfig: CpuConfig,
    val ramConfig: RamConfig,
    val delicateProcesses: List<String>
) {
    companion object {
        fun fromFile(fileName: String): Config? =
            Config::class.java.getResource(fileName)
                ?.readText()
                ?.tryNull { Json.decodeFromString<Config>(it) }
    }
}


@Serializable
data class CpuConfig(val maxTmp: Int, val maxPower: Int, val maxLoad: Int)

@Serializable
data class RamConfig(val maxLoad: Int, val maxClock: Int)