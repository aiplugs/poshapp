<template>
    <span class="d-flex">
        <v-menu ref="dateMenu"
                v-model="menuDate"
                :close-on-content-click="true"
                :nudge-right="40"
                transition="scale-transition"
                offset-y
                min-width="290px">
            <template v-slot:activator="{ on }">
                <v-text-field
                    :label="`${label} - Date`" 
                    :hint="hint"
                    :rules="rules"
                    :required="required"
                    :filled="filled" 
                    :rounded="rounded" 
                    :dense="dense"
                    :value="date"
                    readonly
                    v-on="on"
                    class="flex-grow-1"
                ></v-text-field>
            </template>
            <v-date-picker v-model="date" 
                           v-on:input="handleInput"
                           v-on:click:date="$refs.dateMenu.save(date)">
            </v-date-picker>
        </v-menu>
        <v-menu ref="timeMenu" 
                v-model="menuTime"
                :close-on-content-click="false"
                :nudge-right="40"
                transition="scale-transition"
                offset-y
                min-width="290px">
            <template v-slot:activator="{ on }">
                <v-text-field 
                    class="ml-4 flex-grow-1"
                    :label="`${label} - Time`" 
                    :hint="hint"
                    :rules="rules"
                    :required="required"
                    :filled="filled" 
                    :rounded="rounded" 
                    :dense="dense"
                    :value="time"
                    readonly
                    v-on="on"
                ></v-text-field>
            </template>
            <v-time-picker ref="timePicker"
                           v-model="time"
                           v-on:input="handleInput"
                           v-if="menuTime">
                <v-spacer></v-spacer>
                <v-btn text color="primary" v-on:click="menuTime = false">Cancel</v-btn>
                <v-btn text color="primary" v-on:click="$refs.timeMenu.save(time)">OK</v-btn>
            </v-time-picker>
        </v-menu>
    </span>
</template>
<script>
export default {
    props: {
        value: String,
        label: String,
        hint: String,
        rules: Array,
        required: Boolean,
        filled: Boolean,
        rounded: Boolean,
        dense: Boolean,
    },
    data() {
        const { date, time } = this.extractValue(this.value);
        return {
            date,
            time,
            menuDate: false,
            menuTime: false
        }
    },
    watch: {
        value(newValue) {
            const { date, time } = this.extractValue(newValue);
            this.date = date;
            this.time = time;
        }
    },
    methods: {
        extractValue(value) {
            const ticks = Date.parse(value);

            if (isNaN(ticks))
                return { date: null, time: null };

            const d = new Date(ticks);
            const date = d ? `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}` : null;
            const time = d ? `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}` : null;
            return { date, time };
        },
        handleInput(ev) {
            this.menuDate = false;
            this.menuTime = false;
            const d = new Date(`${this.date} ${this.time}`);
            if (!isNaN(d)) {
                this.$emit('input', d.toJSON());
            }
        }
    }
}
</script>