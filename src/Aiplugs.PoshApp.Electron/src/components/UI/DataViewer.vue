<template>
    <div>
        <div v-for="datum in data" :key="datum.$clixml">
            <p class="pl-4 pr-4" v-if="datum.value === null">NULL</p>
            <div v-else-if="isEmbdedHtml(datum.value)" v-html="getEmbdedHtml(datum.value)"></div>
            <p class="pl-4 pr-4" v-else-if="typeof datum.value == 'string'||typeof datum.value == 'number'">{{datum.value}}</p>
            <p class="pl-4 pr-4" v-else-if="typeof datum.value == 'object' && datum.value.constructor == Date">{{datum.value.toLocaleString()}}</p>
            <v-simple-table class="pb-8" v-else-if="typeof datum.value == 'object' && Array.isArray(datum.value)">
                <template v-slot:default>
                    <thead>
                        <tr>
                            <th v-for="key in getHeaders(datum.value)" :key="key">{{key}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(item, index) in datum.value" :key="index">
                            <td v-for="key in Object.keys(item)" :key="key">{{item[key]}}</td>
                        </tr>
                    </tbody>
                </template>
            </v-simple-table>
            <v-simple-table class="pb-8" v-else-if="typeof datum.value == 'object'">
                <template v-slot:default>
                    <thead>
                        <tr>
                            <th class="text-left">Name</th>
                            <th class="text-left">Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="key in Object.keys(datum.value)" :key="key">
                            <td>{{key}}</td>
                            <td>{{datum.value[key]}}</td>
                        </tr>
                    </tbody>
                </template>
            </v-simple-table>
        </div>
    </div>
</template>
<script>
const htmlheader = /<!--\s*poshapp:\s*(\d+%?)\s*x\s*(\d+%?)\s*-->/
export default {
    props: ['data'],
    methods: {
        getHeaders(array) {
            return [...new Set(array.map(item => Object.keys(item)).flat())];
        },
        isEmbdedHtml(value) {
            return typeof value === 'string' && htmlheader.test(value);
        },
        getEmbdedHtml(value) {
            const [_, width, height] = htmlheader.exec(value);
            const uri = URL.createObjectURL(new Blob([value], { type: 'text/html' }));
            return `<iframe src="${uri}" width="${width}" height="${height}" frameborder="0"></iframe>`;
        }
    }
}
</script>