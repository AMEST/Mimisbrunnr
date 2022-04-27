<template>
  <b-card class="profile-info">
    <span class="text-muted"> Additional information </span>
    <b-card-text>
      <b-input-group-prepend is-text>
        <b-icon icon="bag-fill" /> &nbsp;
        <b-form-input :disabled="!this.itsMe" placeholder="Post"></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon icon="diagram2" /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          placeholder="Department"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon icon="building" /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          placeholder="Organization"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon icon="geo-alt" /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          placeholder="Location"
        ></b-form-input>
      </b-input-group-prepend>
    </b-card-text>
  </b-card>
</template>

<script>
export default {
  name: "AdditionalInfo",
};
</script>

<style>
</style>