<template>
    <v-dialog :value="prompt != null" max-width="600px" persistent>
        <v-card v-if="prompt != null">
            <v-card-title class="headline" primary-title>
                {{prompt.caption}}
            </v-card-title>
            <v-card-text>
                <p style="white-space: pre-wrap">{{prompt.message}}</p>
                <v-form ref="form" v-model="valid">
                    <div v-for="(desc, index) in prompt.descriptions" :key="index">
                        <v-text-field ref="fields"
                                      :label="desc.Label||desc.Name"
                                      :data-name="desc.Name"
                                      :hint="desc.HelpMessage"
                                      :rules="rules(desc)"
                                      :required="desc.IsMandatory"
                                      v-model="value[desc.Name]"
                                      v-if="textfieldTypes.includes(desc.ParameterTypeFullName)">
                            <template v-slot:append v-if="supportFileManager(desc)">
                                <v-icon icon text v-on:click="openDirManager(desc)">mdi-folder-search</v-icon>
                                <v-icon icon text v-on:click="openFileManager(desc)">mdi-file-search</v-icon>
                            </template>
                        </v-text-field>
                        <v-switch :label="(desc.Label||desc.Name)"
                                  :hint="desc.HelpMessage"
                                  :rules="rules(desc)"
                                  :required="desc.IsMandatory"
                                  inset
                                  v-model="value[desc.Name]"
                                  v-else-if="desc.ParameterTypeFullName == 'System.Boolean'">
                        </v-switch>
                        <DateTimeField :label="(desc.Label||desc.Name)"
                                        :hint="desc.helpMessage"
                                        :rules="rules(desc)"
                                        :required="desc.IsMandatory"
                                        v-model="value[desc.Name]"
                                        v-else-if="desc.ParameterTypeFullName == 'System.DateTime'">
                        </DateTimeField>
                    </div>
                </v-form>
            </v-card-text>
            <v-divider></v-divider>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn text :disabled="!valid" v-on:click="handlePrompt">OK</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script>
import {mapState, mapActions} from 'vuex'
import {createCliXml} from '../../clixml.js'
import DateTimeField from './DateTime'
export default {
    components: {DateTimeField},
    data() {
        return {
            valid: false,
            value: {},
            menu: {},
            textfieldTypes: ['System.String','System.Byte','System.SByte','System.Int16','System.Int32','System.Int64','System.Float','System.Double','System.Decimal']
        }
    },
    computed: {
        ...mapState('signalr', ['prompt']),
        ...mapState('ipc', ['selectedDirectory', 'selectedFile'])
    },
    methods: {
        ...mapActions('signalr', ['invokePrompt']),
        ...mapActions('ipc', ['selectDirectory', 'selectFile', 'clearSelected']),
        supportFileManager(desc) {
            return desc.ParameterTypeFullName === 'System.String' && desc.Name.toLowerCase().endsWith('path');
        },
        rules(desc) {
            const result = [];
            const label = desc.Label||desc.Name;

            const optional = function (f) {
                return v => v === null || v === undefined || f(v);
            }

            if (desc.IsMandatory)
                result.push(v => !!v || `${label} is required`);

            if (desc.ParameterTypeFullName == 'System.Int32'
                || desc.ParameterTypeFullName == 'System.Int64')
                result.push(optional(v => !isNaN(parseInt(v)) || `${label} need integer value`))

            if (desc.ParameterTypeFullName == 'System.Float'
                || desc.ParameterTypeFullName == 'System.Double'
                || desc.ParameterTypeFullName == 'System.Decimal')
                result.push(optional(v => !isNaN(parseFloat(v)) || `${label} need number value`))

            return result;
        },
        handlePrompt() {
            const input = {};
            for (let desc of this.prompt.descriptions) {
                let value = null;
                if (desc.ParameterTypeFullName == 'System.DateTime') {
                    value = new Date(this.value[desc.Name]).toJSON();
                }
                else {
                    value = this.value[desc.Name];
                }
                input[desc.Name] = createCliXml(desc.ParameterTypeFullName, value);
            }
            this.invokePrompt({ input });
            this.clearSelected();
            this.$refs.form.reset();
            this.dialog = false;
        },
        openDirManager(desc) {
            this.selectDirectory({ name: desc.Name });
        }, 
        openFileManager(desc) {
            this.selectFile({ name: desc.Name });
        }
    },
    watch: {
        prompt(o) {
            if (o != null) {
                for (let desc of o.descriptions) {
                    this.$set(this.value, desc.Name, desc.DefaultValue);
                }
            }
        },
        selectedDirectory(o) {
            if (this.prompt != null) {
                const keys = Object.keys(o).filter(key => !key.startsWith('_'));
                for (let desc of this.prompt.descriptions) {
                    if (keys.includes(desc.Name)) {
                        this.$set(this.value, desc.Name, o[desc.Name]);
                    }
                }
            }
        },
        selectedFile(o) {
            if (this.prompt != null) {
                const keys = Object.keys(o).filter(key => !key.startsWith('_'));
                for (let desc of this.prompt.descriptions) {
                    if (keys.includes(desc.Name)) {
                        this.$set(this.value, desc.Name, o[desc.Name]);
                    }
                }
            }
        }
    }
}
</script>