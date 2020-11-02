<template>
    <v-form v-model="valid">
        <v-row>
            <v-col class="pb-0" v-for="p in parameters" :key="p.name" :md="colSize(p)">
                <DateTimeField
                    v-model="value[p.name]"
                    :label="p.name"
                    :hint="p.helpMessage"
                    :rules="rules(p)"
                    :disabled="disabledAll"
                    filled rounded dense
                    v-if="p.type == 'System.DateTimeOffset' || p.type == 'System.DateTime'">
                </DateTimeField>
                <v-select
                    v-model="value[p.name]"
                    :label="p.name"
                    :hint="p.helpMessage"
                    :items="validateSet(p)"
                    :disabled="disabledAll"
                    v-else-if="p.validateSet">
                </v-select>
                <v-switch
                    v-model="value[p.name]"
                    :label="p.name"
                    :hint="p.helpMessage"
                    :rules="rules(p)"
                    :disabled="disabledAll"
                    inset 
                    v-else-if="p.type == 'System.Boolean'">
                </v-switch>
                <v-text-field 
                    v-model="value[p.name]" 
                    :label="p.name" 
                    :hint="p.helpMessage"
                    :type="inputType(p)" 
                    :rules="rules(p)" 
                    :disabled="disabledAll"
                    filled rounded dense v-else>
                    <template v-slot:append v-if="supportFileManager(p)">
                        <v-icon icon text v-on:click="openDirManager(p)">mdi-folder-search</v-icon>
                        <v-icon icon text v-on:click="openFileManager(p)">mdi-file-search</v-icon>
                    </template>
                </v-text-field>
            </v-col>
            <v-col class="pb-0" v-if="loadingParams">
                <v-text-field filled rounded dense disabled></v-text-field>
            </v-col>
            <v-col class="pb-0" style="height:92px;" v-if="!loadingParams&&(parameters.length==0||parameters.every(p => isNumberType(p.type)))">
            </v-col>
            <div class="pa-3 pb-0">
                <v-btn text x-large :disabled="disabledAll||!valid" :loading="loading" v-on:click="emitRun">Run</v-btn>
            </div>
        </v-row>
    </v-form>
</template>
<script>
import {mapState, mapActions} from 'vuex'
import { createCliXml } from '../../clixml.js'
import DateTimeField from './DateTime' 
export default {
    props: {
        script: String,
        page: Number,
        pageSize: Number,
        disabled: Boolean,
        loading: Boolean
    },
    components: {DateTimeField},
    data() {
        return {
            valid: true,
            value: { Page: this.page - 1, PageSize: this.pageSize }
        }
    },
    computed: {
        ...mapState('ipc', ['loadingParams', 'parameters']),
        disabledAll() {
            return this.disabled || this.loadingParams
        }
    },
    watch: {
        page(value) {
            const page = value - 1;
            if (this.value.Page != page) {
                this.value.Page = page;
                this.emitRun();
            }
        },
        pageSize(value) {
            const pageSize = value - 0;
            if (this.value.PageSize != pageSize) {
                this.value.PageSize = pageSize;
                this.emitRun();
            }
        },
        script() {
            this.invokeGetParameters({ scriptId: this.script });
        },
        parameters(parameters) {
            for (let p of parameters) {
                this.value[p.name] = p.defaultValue || this.value[p.name];
            }
        }
    },
    methods: {
        ...mapActions('ipc', ['invokeGetParameters']),
        isNumberType(type) {
            const numberTypes = ['System.Int32','System.Int64','System.Single','System.Double','System.Decimal'];
            return numberTypes.includes(type);
        },
        inputType(parameter) {
            const numberTypes = ['System.Int32','System.Int64','System.Single','System.Double','System.Decimal'];
            if (numberTypes.includes(parameter.type)) {
                return 'number';
            }
            return 'text';
        },
        rules(parameter) {
            const rules = [];
            const optional = function (f) {
                return v => v === null || v === undefined || f(v);
            }

            if (parameter.mandatory)
                rules.push(v => v !== undefined || v !== null || v !== '' || `${parameter.name} is required.`);

            if (parameter.type == 'System.Int32'
                || parameter.type == 'System.Int64')
                rules.push(optional(v => !isNaN(parseInt(v)) || `${parameter.name} need integer value`))

            if (parameter.type == 'System.Float'
                || parameter.type == 'System.Double'
                || parameter.type == 'System.Decimal')
                rules.push(optional(v => !isNaN(parseFloat(v)) || `${parameter.name} need number value`))

            if (parameter.validateLength != null && parameter.validateLength.length == 2)
                rules.push(optional(v => v && parameter.validateLength[0] <= v.length && v.length <= parameter.validateLength[1] || `${parameter.name} must be a string with a length between ${parameter.validateLength[0]} and ${parameter.validateLength[1]}.`))

            if (parameter.validateRange != null && parameter.validateRange.length == 2)
                rules.push(optional(v => parameter.validateRange[0] <= v && v <= parameter.validateRange[1] || `${parameter.name} need a number between ${parameter.validateRange[0]} and ${parameter.validateRange[1]}`))

            if (parameter.validatePattern != null)
                rules.push(optional(v => new RegExp(parameter.validatePattern).test(v) || `${parameter.name} need ${parameter.validatePattern}`))

            return rules;
        },
        colSize(parameter) {
            return this.isNumberType(parameter.type) || parameter.validateSet ? 2 : null;
        },
        validateSet(parameter) {
            return parameter.validateSet.map(v => ({ text: v, value: v }));
        },
        getClixmls() {
            return this.parameters.reduce((o, p) => {
                if (this.value[p.name] !== undefined) {
                    o[p.name] = createCliXml(p.type, this.value[p.name]);
                }
                return o;
            }, {});
        },
        emitRun() {
            this.$emit('run', {
                value: this.getClixmls(),
                page: this.value.Page - 0 + 1,
                pageSize: this.value.PageSize - 0
            });
        },
        supportFileManager(p) {
            return p.type === 'System.String' && p.name.toLowerCase().endsWith('path');
        },
        async openDirManager(p) {
            this.$set(this.value, p.name, await window.selectDirectory({ name: p.name }));
        }, 
        async openFileManager(p) {
            this.$set(this.value, p.name, await window.selectFile({ name: p.name }));
        }
    },
    mounted() {
        this.invokeGetParameters({ scriptId: this.script });
    }
}
</script>